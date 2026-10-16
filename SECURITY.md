# Security Policy

## Supported versions

Only the latest code on the `master` branch (the deployed version) receives security fixes.

## Reporting a vulnerability

**Please do not report security vulnerabilities through public GitHub issues, pull requests, or discussions.**

Email **zoozxc456@gmail.com** with the subject line `[ChoicePie Security]` and include:

- A description of the issue and its potential impact
- The affected component (frontend, Nuxt BFF, backend API, SignalR game hub, admin)
- Steps to reproduce, or a proof of concept
- Any relevant URLs, requests, or logs (please redact real user data)

### What to expect

- Acknowledgement of your report within **3 business days**
- An initial assessment within **7 business days**
- Updates as the fix progresses, and a note when it has been released

Please give us a reasonable amount of time to fix the issue before disclosing it publicly.

## Scope

In scope:

- Authentication and session handling (JWT, refresh tokens, auth cookies, Google sign-in)
- Authorization bypass (member / admin boundaries, editing other users' quizzes)
- Game hub abuse (joining or controlling rooms you don't own, tampering with scores)
- Injection, XSS, CSRF, and SSRF (including the AI quiz generation URL fetcher)
- Exposure of personal data or secrets

Out of scope:

- Denial-of-service or load testing
- Social engineering or physical attacks
- Reports from automated scanners without a demonstrated impact
- Missing security headers or best practices without a concrete exploit

## Safe harbor

We will not take legal action against good-faith research that follows this policy: avoid privacy violations and service disruption, only test against accounts you own, and do not access, modify, or keep other users' data.
