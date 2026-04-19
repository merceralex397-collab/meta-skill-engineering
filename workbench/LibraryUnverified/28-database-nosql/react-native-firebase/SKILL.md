---
name: react-native-firebase
description: >-
  Integrate Firebase Auth, Firestore, and FCM with React Native apps. Use when
  setting up Firebase in React Native, implementing auth flows with
  @react-native-firebase, reading/writing Firestore data with real-time
  listeners, or configuring push notifications with FCM. Do not use for
  web-only Firebase or non-Firebase mobile backends.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: react-native-firebase
  maturity: draft
  risk: low
  tags: [react-native, firebase, auth, firestore]
---

# Purpose

Integrate Firebase services (Auth, Firestore, FCM) with React Native using `@react-native-firebase` for authentication, real-time data, and push notifications.

# When to use this skill

- setting up `@react-native-firebase` in a React Native project
- implementing email/Google/Apple auth flows with Firebase Auth
- reading and writing Firestore data with real-time listeners
- configuring push notifications with Firebase Cloud Messaging

# Do not use this skill when

- building a web-only app with Firebase — use Firebase JS SDK patterns
- using a non-Firebase backend (Supabase, Amplify) — different APIs
- the task is React Native UI without Firebase — prefer `react-typescript`

# Procedure

1. **Install packages** — `npx expo install @react-native-firebase/app @react-native-firebase/auth @react-native-firebase/firestore`.
2. **Configure native** — add `google-services.json` (Android) and `GoogleService-Info.plist` (iOS) to native project dirs.
3. **Set up Auth** — use `auth().signInWithEmailAndPassword()`, `auth().createUserWithEmailAndPassword()`. Listen with `auth().onAuthStateChanged()`.
4. **Set up Firestore** — read with `firestore().collection('users').doc(uid).get()`. Write with `.set()` or `.update()`.
5. **Add real-time listeners** — `firestore().collection('messages').orderBy('createdAt').onSnapshot(snap => ...)`. Clean up in `useEffect` return.
6. **Configure FCM** — request permission with `messaging().requestPermission()`. Get token: `messaging().getToken()`. Handle with `messaging().onMessage()`.
7. **Security rules** — write Firestore rules that check `request.auth.uid`. Test with Firebase Emulator.
8. **Test offline** — Firestore has offline persistence by default. Verify reads work without network.

# Auth flow

```tsx
import auth from '@react-native-firebase/auth';
import { useEffect, useState } from 'react';

function useAuth() {
  const [user, setUser] = useState(auth().currentUser);

  useEffect(() => {
    return auth().onAuthStateChanged(setUser);
  }, []);

  return user;
}

async function signIn(email: string, password: string) {
  try {
    await auth().signInWithEmailAndPassword(email, password);
  } catch (err: any) {
    if (err.code === 'auth/user-not-found') throw new Error('No account found');
    if (err.code === 'auth/wrong-password') throw new Error('Incorrect password');
    throw err;
  }
}
```

# Firestore patterns

```tsx
import firestore from '@react-native-firebase/firestore';

// Real-time listener with cleanup
function useMessages(chatId: string) {
  const [messages, setMessages] = useState([]);

  useEffect(() => {
    const unsubscribe = firestore()
      .collection('chats').doc(chatId).collection('messages')
      .orderBy('createdAt', 'desc')
      .limit(50)
      .onSnapshot(snap => {
        setMessages(snap.docs.map(d => ({ id: d.id, ...d.data() })));
      });
    return unsubscribe;
  }, [chatId]);

  return messages;
}
```

# Decision rules

- Always unsubscribe from Firestore listeners in `useEffect` cleanup — prevents memory leaks.
- Use `auth().onAuthStateChanged()` as the single source of auth truth — not manual state.
- Structure Firestore as shallow collections — avoid deep nesting (max 1 subcollection level).
- Store FCM tokens in Firestore under the user document — enables server-side push targeting.
- Test with Firebase Emulator Suite locally — do not test against production.

# References

- https://rnfirebase.io/
- https://firebase.google.com/docs/firestore/security/overview

# Related skills

- `react-typescript` — React component patterns
- `state-management` — client state alongside Firestore
- `forms-validation` — auth form validation
