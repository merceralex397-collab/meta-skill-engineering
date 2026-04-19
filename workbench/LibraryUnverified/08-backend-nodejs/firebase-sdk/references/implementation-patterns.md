# Implementation Patterns — Firebase SDK

## Admin SDK initialization

### Node.js — Application Default Credentials (Cloud Functions, GCP)

```javascript
const admin = require('firebase-admin');
admin.initializeApp();  // Uses ADC automatically in GCP environments
const db = admin.firestore();
const auth = admin.auth();
```

### Node.js — Explicit service account (local development, non-GCP)

```javascript
const admin = require('firebase-admin');
const serviceAccount = require('./service-account-key.json');
admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
  storageBucket: 'my-project.appspot.com',
});
```

**Never commit service account keys to git.** Use environment variables or secret managers.

### Python Admin SDK

```python
import firebase_admin
from firebase_admin import credentials, firestore

# With ADC
firebase_admin.initialize_app()

# With explicit credentials
cred = credentials.Certificate('service-account-key.json')
firebase_admin.initialize_app(cred)

db = firestore.client()
```

## Client SDK v9 modular imports

Always import specific functions, never the entire module:

```javascript
// Good — tree-shakeable
import { getFirestore, collection, doc, getDoc, getDocs } from 'firebase/firestore';
import { getAuth, signInWithEmailAndPassword } from 'firebase/auth';

// Bad — imports entire SDK, no tree shaking
import firebase from 'firebase/compat/app';
import 'firebase/compat/firestore';
```

## Firestore query composition

Queries are built by chaining constraints:

```javascript
import { collection, query, where, orderBy, limit, startAfter, getDocs } from 'firebase/firestore';

// Basic query
const q = query(
  collection(db, 'products'),
  where('category', '==', 'electronics'),
  where('price', '<=', 1000),
  orderBy('price', 'asc'),
  limit(25)
);

// Paginated query — pass the last document snapshot
const nextQ = query(
  collection(db, 'products'),
  where('category', '==', 'electronics'),
  orderBy('price', 'asc'),
  startAfter(lastVisibleDoc),
  limit(25)
);

const snapshot = await getDocs(nextQ);
```

### Index requirements

Queries that combine `where` on one field with `orderBy` on a different field require a composite index. Firestore will return a `FAILED_PRECONDITION` error with a direct link to create the index.

Add indexes to `firestore.indexes.json`:
```json
{
  "indexes": [
    {
      "collectionGroup": "products",
      "queryScope": "COLLECTION",
      "fields": [
        { "fieldPath": "category", "order": "ASCENDING" },
        { "fieldPath": "price", "order": "ASCENDING" }
      ]
    }
  ]
}
```

Deploy: `firebase deploy --only firestore:indexes`

## Batch write patterns

```javascript
import { writeBatch, doc, serverTimestamp } from 'firebase/firestore';

async function batchCreatePosts(posts) {
  // Firestore batches are limited to 500 operations
  const BATCH_SIZE = 500;
  for (let i = 0; i < posts.length; i += BATCH_SIZE) {
    const batch = writeBatch(db);
    const chunk = posts.slice(i, i + BATCH_SIZE);
    for (const post of chunk) {
      const ref = doc(collection(db, 'posts'));
      batch.set(ref, { ...post, createdAt: serverTimestamp() });
    }
    await batch.commit();
  }
}
```

## Transaction retry behavior

Transactions auto-retry up to 5 times if a read document is modified during the transaction. Keep transactions short and focused.

```javascript
import { runTransaction, doc, serverTimestamp } from 'firebase/firestore';

async function transferCredits(fromId, toId, amount) {
  await runTransaction(db, async (tx) => {
    const fromRef = doc(db, 'wallets', fromId);
    const toRef = doc(db, 'wallets', toId);
    const fromSnap = await tx.get(fromRef);
    const toSnap = await tx.get(toRef);

    if (!fromSnap.exists() || !toSnap.exists()) throw new Error('Wallet not found');
    if (fromSnap.data().balance < amount) throw new Error('Insufficient balance');

    tx.update(fromRef, { balance: fromSnap.data().balance - amount, updatedAt: serverTimestamp() });
    tx.update(toRef, { balance: toSnap.data().balance + amount, updatedAt: serverTimestamp() });
  });
}
```

## onSnapshot listener management

### React pattern with useEffect

```javascript
import { useEffect, useState } from 'react';
import { collection, query, where, orderBy, onSnapshot } from 'firebase/firestore';

function useMessages(roomId) {
  const [messages, setMessages] = useState([]);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!roomId) return;
    const q = query(
      collection(db, 'messages'),
      where('roomId', '==', roomId),
      orderBy('timestamp', 'asc')
    );
    const unsubscribe = onSnapshot(q,
      (snap) => setMessages(snap.docs.map(d => ({ id: d.id, ...d.data() }))),
      (err) => setError(err)
    );
    return () => unsubscribe();  // Cleanup on unmount or roomId change
  }, [roomId]);

  return { messages, error };
}
```

### Multiple listeners with cleanup tracking

```javascript
class ListenerManager {
  constructor() {
    this.unsubscribers = new Map();
  }

  subscribe(key, queryRef, callback, errorCallback) {
    this.unsubscribe(key);  // Clean up existing listener for this key
    const unsub = onSnapshot(queryRef, callback, errorCallback);
    this.unsubscribers.set(key, unsub);
  }

  unsubscribe(key) {
    const unsub = this.unsubscribers.get(key);
    if (unsub) { unsub(); this.unsubscribers.delete(key); }
  }

  unsubscribeAll() {
    this.unsubscribers.forEach(unsub => unsub());
    this.unsubscribers.clear();
  }
}
```

## Cloud Functions v2 patterns

### HTTP callable function

```javascript
const { onCall, HttpsError } = require('firebase-functions/v2/https');
const admin = require('firebase-admin');

exports.createOrder = onCall(async (request) => {
  if (!request.auth) throw new HttpsError('unauthenticated', 'Must be signed in');

  const { productId, quantity } = request.data;
  if (!productId || !quantity) throw new HttpsError('invalid-argument', 'Missing fields');

  const order = await admin.firestore().collection('orders').add({
    productId,
    quantity,
    userId: request.auth.uid,
    status: 'pending',
    createdAt: admin.firestore.FieldValue.serverTimestamp(),
  });

  return { orderId: order.id };
});
```

### Firestore trigger

```javascript
const { onDocumentCreated } = require('firebase-functions/v2/firestore');

exports.onOrderCreated = onDocumentCreated('orders/{orderId}', async (event) => {
  const order = event.data.data();
  // Send confirmation email, update inventory, etc.
  await admin.firestore().doc(`users/${order.userId}/notifications/${event.params.orderId}`).set({
    message: `Order ${event.params.orderId} received`,
    createdAt: admin.firestore.FieldValue.serverTimestamp(),
  });
});
```
