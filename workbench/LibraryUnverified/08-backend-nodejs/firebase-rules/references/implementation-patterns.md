# Implementation Patterns — Firebase Security Rules

## Owner-only access

The most common pattern. Users can only read and write their own documents.

```
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /users/{userId} {
      allow get: if request.auth != null && request.auth.uid == userId;
      allow list: if false;  // Users cannot list other users
      allow create: if request.auth != null && request.auth.uid == userId;
      allow update: if request.auth != null && request.auth.uid == userId;
      allow delete: if false;  // Users cannot delete their own accounts via client
    }
  }
}
```

## Role-based access with custom claims

Set custom claims via Admin SDK, then check in rules. Claims are in `request.auth.token`.

```
function isAdmin() {
  return request.auth != null && request.auth.token.admin == true;
}

function isModerator() {
  return request.auth != null && request.auth.token.role == 'moderator';
}

match /posts/{postId} {
  allow read: if request.auth != null;
  allow create: if request.auth != null;
  allow update: if isOwner(resource.data.authorId) || isModerator();
  allow delete: if isAdmin();
}
```

Setting claims server-side (Node.js Admin SDK):

```javascript
const admin = require('firebase-admin');
await admin.auth().setCustomUserClaims(uid, { admin: true, role: 'moderator' });
// User must refresh their token after this
```

## Field validation on create

Ensure required fields exist and have correct types.

```
match /orders/{orderId} {
  allow create: if request.auth != null
    && request.resource.data.keys().hasAll(['product', 'quantity', 'userId'])
    && request.resource.data.product is string
    && request.resource.data.quantity is int
    && request.resource.data.quantity > 0
    && request.resource.data.quantity <= 100
    && request.resource.data.userId == request.auth.uid
    && request.resource.data.keys().hasOnly(['product', 'quantity', 'userId', 'notes']);
}
```

## Field validation on update — restrict which fields can change

```
match /users/{userId} {
  allow update: if request.auth.uid == userId
    && request.resource.data.diff(resource.data).affectedKeys()
        .hasOnly(['displayName', 'photoURL', 'bio'])
    && request.resource.data.displayName is string
    && request.resource.data.displayName.size() <= 50;
}
```

## Granular read: get vs list

```
match /profiles/{profileId} {
  // Anyone authenticated can look up a specific profile
  allow get: if request.auth != null;
  // Only admins can list/search all profiles
  allow list: if isAdmin();
}
```

## Helper function library

```
function isSignedIn() {
  return request.auth != null;
}

function isOwner(ownerId) {
  return isSignedIn() && request.auth.uid == ownerId;
}

function hasRole(role) {
  return isSignedIn() && request.auth.token.role == role;
}

function isValidString(field, minLen, maxLen) {
  return field is string && field.size() >= minLen && field.size() <= maxLen;
}

function isValidTimestamp(field) {
  return field is timestamp;
}
```

## Cross-document lookup with get()

Use sparingly — limited to 10 calls per rules evaluation.

```
match /comments/{commentId} {
  allow create: if request.auth != null
    && exists(/databases/$(database)/documents/posts/$(request.resource.data.postId))
    && get(/databases/$(database)/documents/posts/$(request.resource.data.postId)).data.commentsEnabled == true;
}
```

## Cloud Storage rules — file type and size validation

```
rules_version = '2';
service firebase.storage {
  match /b/{bucket}/o {
    match /user-uploads/{userId}/{fileName} {
      allow read: if request.auth != null;
      allow write: if request.auth != null
        && request.auth.uid == userId
        && request.resource.size < 5 * 1024 * 1024  // 5MB limit
        && request.resource.contentType.matches('image/(png|jpeg|gif|webp)');
    }
  }
}
```

## Nested subcollection access

```
match /teams/{teamId} {
  allow read: if request.auth != null
    && exists(/databases/$(database)/documents/teams/$(teamId)/members/$(request.auth.uid));

  match /members/{memberId} {
    allow read: if request.auth != null
      && exists(/databases/$(database)/documents/teams/$(teamId)/members/$(request.auth.uid));
    allow write: if request.auth != null
      && get(/databases/$(database)/documents/teams/$(teamId)/members/$(request.auth.uid)).data.role == 'admin';
  }

  match /messages/{messageId} {
    allow read: if request.auth != null
      && exists(/databases/$(database)/documents/teams/$(teamId)/members/$(request.auth.uid));
    allow create: if request.auth != null
      && exists(/databases/$(database)/documents/teams/$(teamId)/members/$(request.auth.uid))
      && request.resource.data.authorId == request.auth.uid;
  }
}
```

## Timestamp enforcement — server timestamp only

```
match /events/{eventId} {
  allow create: if request.auth != null
    && request.resource.data.createdAt == request.time;
  allow update: if request.auth != null
    && request.resource.data.updatedAt == request.time
    && request.resource.data.createdAt == resource.data.createdAt;  // Cannot change createdAt
}
```
