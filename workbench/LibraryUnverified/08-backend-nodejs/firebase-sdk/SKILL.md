---
name: firebase-sdk
description: >-
  Initialize and use Firebase client SDK (v9 modular) and Admin SDK for Firestore CRUD,
  Auth flows, Cloud Storage, real-time listeners, transactions, batch writes, and Cloud Functions.
  Use when writing addDoc/getDoc/setDoc/updateDoc/deleteDoc calls, setting up onSnapshot listeners,
  implementing signInWithEmailAndPassword or signInWithPopup, or configuring Admin SDK in server environments.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: firebase-sdk
  maturity: draft
  risk: medium
  tags: [firebase, sdk, firestore, auth, cloud-functions, storage]
---

# Purpose

Initialize and use the Firebase client SDK (v9 modular API) and Admin SDK (Node.js/Python) for Firestore CRUD, Firebase Auth, Cloud Storage, real-time listeners, transactions, batch writes, and Cloud Functions. This skill covers correct initialization, query composition, listener lifecycle management, and offline persistence configuration.

# When to use this skill

Use this skill when:

- initializing Firebase client SDK (`initializeApp`) or Admin SDK (`admin.initializeApp`)
- performing Firestore CRUD operations (addDoc, getDoc, setDoc, updateDoc, deleteDoc, getDocs)
- composing Firestore queries with where, orderBy, limit, startAfter
- setting up real-time listeners with onSnapshot
- implementing Firebase Auth flows (email/password, OAuth, onAuthStateChanged)
- working with Cloud Storage (uploadBytes, getDownloadURL)
- writing Cloud Functions (onCall, onRequest, Firestore triggers)
- using transactions or batch writes for atomic operations
- configuring offline persistence

# Do not use this skill when

- the task is about writing or testing Firestore security rules — use `firebase-rules` instead
- the task is about BigQuery export or analytics queries — use `bigquery` instead
- the task is purely about data model design without SDK code — use `data-model` instead
- the task is about WebSocket connections not using Firebase — use `realtime-websocket` instead

# Operating procedure

1. **Determine the environment.** Client (browser/React Native) uses the modular v9 SDK. Server (Node.js Cloud Functions, scripts) uses the Admin SDK. Never use Admin SDK in client code.

2. **Initialize the SDK.**

   Client SDK (v9 modular):
   ```javascript
   import { initializeApp } from 'firebase/app';
   import { getFirestore } from 'firebase/firestore';
   import { getAuth } from 'firebase/auth';

   const app = initializeApp({
     apiKey: '...',
     authDomain: '...',
     projectId: '...',
     storageBucket: '...',
     messagingSenderId: '...',
     appId: '...',
   });
   const db = getFirestore(app);
   const auth = getAuth(app);
   ```

   Admin SDK (Node.js):
   ```javascript
   const admin = require('firebase-admin');
   // In Cloud Functions or GCP — uses Application Default Credentials
   admin.initializeApp();
   // Or with explicit service account
   admin.initializeApp({
     credential: admin.credential.cert(serviceAccount),
   });
   const db = admin.firestore();
   ```

3. **Perform Firestore operations.**

   ```javascript
   import { collection, doc, addDoc, getDoc, getDocs, setDoc, updateDoc, deleteDoc, query, where, orderBy, limit } from 'firebase/firestore';

   // Create
   const docRef = await addDoc(collection(db, 'posts'), { title: 'Hello', author: uid });

   // Read single
   const snap = await getDoc(doc(db, 'posts', postId));
   if (snap.exists()) { console.log(snap.data()); }

   // Read with query
   const q = query(collection(db, 'posts'), where('author', '==', uid), orderBy('createdAt', 'desc'), limit(20));
   const querySnap = await getDocs(q);
   querySnap.forEach(doc => console.log(doc.id, doc.data()));

   // Update
   await updateDoc(doc(db, 'posts', postId), { title: 'Updated' });

   // Delete
   await deleteDoc(doc(db, 'posts', postId));
   ```

4. **Set up real-time listeners.** Always store the unsubscribe function and call it on cleanup.

   ```javascript
   import { onSnapshot } from 'firebase/firestore';

   const unsubscribe = onSnapshot(
     query(collection(db, 'messages'), where('roomId', '==', roomId), orderBy('timestamp')),
     (snapshot) => {
       snapshot.docChanges().forEach(change => {
         if (change.type === 'added') handleNewMessage(change.doc.data());
         if (change.type === 'modified') handleUpdatedMessage(change.doc.data());
         if (change.type === 'removed') handleRemovedMessage(change.doc.data());
       });
     },
     (error) => { console.error('Listener error:', error); }
   );

   // On component unmount or cleanup:
   unsubscribe();
   ```

5. **Implement Auth flows.**

   ```javascript
   import { signInWithEmailAndPassword, signInWithPopup, GoogleAuthProvider, onAuthStateChanged, signOut } from 'firebase/auth';

   // Email/password
   const { user } = await signInWithEmailAndPassword(auth, email, password);

   // Google OAuth
   const provider = new GoogleAuthProvider();
   const { user } = await signInWithPopup(auth, provider);

   // Listen for auth state
   const unsubscribe = onAuthStateChanged(auth, (user) => {
     if (user) { /* signed in */ } else { /* signed out */ }
   });

   // Sign out
   await signOut(auth);
   ```

6. **Use transactions for atomic operations.**

   ```javascript
   import { runTransaction } from 'firebase/firestore';

   await runTransaction(db, async (transaction) => {
     const counterRef = doc(db, 'counters', 'visits');
     const counterSnap = await transaction.get(counterRef);
     const newCount = (counterSnap.data()?.count || 0) + 1;
     transaction.update(counterRef, { count: newCount });
   });
   ```

7. **Use batch writes for multiple operations.**

   ```javascript
   import { writeBatch } from 'firebase/firestore';

   const batch = writeBatch(db);
   batch.set(doc(db, 'posts', id1), { title: 'Post 1' });
   batch.update(doc(db, 'posts', id2), { views: 100 });
   batch.delete(doc(db, 'posts', id3));
   await batch.commit();  // Atomic — all succeed or all fail
   ```

8. **Configure offline persistence (web).**

   ```javascript
   import { enableIndexedDbPersistence } from 'firebase/firestore';
   // Call once after getFirestore, before any reads/writes
   enableIndexedDbPersistence(db).catch((err) => {
     if (err.code === 'failed-precondition') { /* Multiple tabs open */ }
     if (err.code === 'unimplemented') { /* Browser doesn't support */ }
   });
   ```

9. **Deploy and test.** Use the Firebase Emulator Suite for local development:
   ```bash
   firebase emulators:start --only firestore,auth,functions,storage
   ```
   Point SDK at emulators in development:
   ```javascript
   import { connectFirestoreEmulator } from 'firebase/firestore';
   import { connectAuthEmulator } from 'firebase/auth';
   connectFirestoreEmulator(db, 'localhost', 8080);
   connectAuthEmulator(auth, 'http://localhost:9099');
   ```

# Decision rules

- Use v9 modular imports for new code — they enable tree-shaking and reduce bundle size. Only use compat (`firebase/compat`) when migrating legacy codebases.
- Use Admin SDK exclusively in trusted server environments (Cloud Functions, backend servers). Never bundle `firebase-admin` in client code.
- Prefer `addDoc` when you want auto-generated IDs; use `setDoc` when you need a specific document ID.
- Always attach an error callback to `onSnapshot` — without it, listener errors are silently swallowed.
- Unsubscribe all listeners on component unmount or route change to prevent memory leaks.
- Use transactions when reads and writes must be atomic. Use batch writes when you have multiple independent writes that should all succeed or all fail.
- Create composite indexes for queries that combine `where` with `orderBy` on different fields — Firestore will throw FAILED_PRECONDITION without them.
- For pagination, use `startAfter(lastDoc)` with a consistent `orderBy`, not offset-based pagination.

# Output requirements

1. `SDK Setup` — initialization code for the correct environment (client vs. server)
2. `Data Operations` — Firestore read/write code with proper error handling
3. `Listener Management` — onSnapshot setup with unsubscribe cleanup
4. `Index Requirements` — any composite indexes needed (for firestore.indexes.json)

# References

Read these when working on specific aspects:

- `references/implementation-patterns.md` — SDK initialization, query patterns, listener management
- `references/validation-checklist.md` — security and correctness checks
- `references/failure-modes.md` — common errors and their fixes

# Related skills

- `firebase-rules` — security rules that govern what SDK operations are allowed
- `data-model` — Firestore document/collection schema design
- `realtime-websocket` — alternative real-time approaches beyond Firestore listeners
- `api-contracts` — API design patterns for Cloud Functions endpoints

# Anti-patterns

- **Using Admin SDK in client-side code.** Admin SDK bypasses all security rules and has full read/write access. Bundling it in a browser app exposes your entire database.
- **Not unsubscribing onSnapshot listeners.** Each active listener holds a connection and receives updates. Forgetting to unsubscribe causes memory leaks and unexpected behavior as listeners accumulate.
- **Using `setDoc` without `{ merge: true }` when you want a partial update.** Plain `setDoc` overwrites the entire document. Use `updateDoc` for partial updates, or `setDoc` with `{ merge: true }`.
- **Ignoring the Firestore index requirement.** Composite queries (where + orderBy on different fields) require indexes. Not creating them causes runtime FAILED_PRECONDITION errors. Check the error message — it contains a direct link to create the index.
- **Reading an entire collection with `getDocs(collection(db, 'users'))`.** This fetches every document and scales poorly. Always use queries with `where`, `limit`, and pagination.
- **Not handling offline state.** With persistence enabled, writes queue locally when offline. The app must handle `fromCache` metadata and inform users about pending writes.
- **Storing sensitive data in client-accessible collections.** Even with rules, prefer to keep truly sensitive data in server-only collections accessed via Cloud Functions.

# Failure handling

- **FAILED_PRECONDITION on query.** Missing composite index. Check the error message for a link to create it in the Firebase Console, or add it to `firestore.indexes.json` and deploy with `firebase deploy --only firestore:indexes`.
- **Auth token expired.** Firebase Auth tokens expire after 1 hour. The SDK automatically refreshes them, but if the refresh fails (network error), operations will fail. Check `onAuthStateChanged` for null user state.
- **Offline cache conflicts.** When persistence is enabled and the device comes back online, local writes are synced. If server-side rules deny the write, the local cache may show data that gets reverted. Handle the `metadata.hasPendingWrites` flag.
- **onSnapshot listener accumulation.** Each call to `onSnapshot` opens a new listener. If called in a React component without cleanup, listeners multiply on re-renders. Always use `useEffect` cleanup or equivalent.
- **Transaction contention.** Transactions retry up to 5 times if the underlying data changes during execution. Under high contention, they may exhaust retries and fail. Use batch writes when operations don't need to read first.
- **CORS on Storage downloads.** `getDownloadURL` returns a URL that may be subject to CORS. Configure CORS on the Storage bucket using `gsutil cors set cors.json gs://your-bucket`.
- **Missing service account permissions.** Admin SDK in Cloud Functions uses the default service account. If custom permissions are needed (e.g., for other GCP services), configure the service account in the Cloud Console.
