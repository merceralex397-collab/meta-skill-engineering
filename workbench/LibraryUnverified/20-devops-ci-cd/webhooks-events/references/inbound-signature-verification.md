# Inbound Signature Verification

Before trusting a webhook payload:

- capture the raw request body exactly as received
- read the provider's signature header and timestamp header
- verify HMAC or provider-specific signature before parsing business content
- reject stale timestamps when the provider contract supports replay windows

Common mistakes:

- verifying the parsed JSON instead of the raw body
- trimming whitespace or re-serializing before verification
- logging the secret-bearing header verbatim
