---
name: seo-structured-data
description: >-
  Add JSON-LD structured data, Open Graph meta, and sitemaps for search
  engines. Use when adding JSON-LD schema markup, configuring OG/Twitter
  meta tags, generating sitemaps, or fixing Google Search Console errors.
  Do not use for analytics tracking (prefer analytics-instrumentation) or
  visual performance (prefer frontend-performance).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: seo-structured-data
  maturity: draft
  risk: low
  tags: [seo, structured-data, json-ld, og]
---

# Purpose

Add JSON-LD structured data, Open Graph meta tags, and sitemaps to improve search engine visibility and social sharing.

# When to use this skill

- adding JSON-LD schema markup (Article, Product, FAQ, Organization)
- configuring Open Graph and Twitter Card meta tags
- generating `sitemap.xml` and `robots.txt`
- fixing Google Search Console structured data errors

# Do not use this skill when

- the task is analytics tracking — prefer `analytics-instrumentation`
- the task is visual performance — prefer `frontend-performance`
- the task is accessibility — prefer `accessibility-audit`

# Procedure

1. **Identify schema types** — match content to schema.org types: Article, Product, FAQ, BreadcrumbList, Organization, LocalBusiness.
2. **Add JSON-LD** — insert `<script type="application/ld+json">` in `<head>`. One script per schema type.
3. **Add OG meta tags** — `og:title`, `og:description`, `og:image`, `og:url`, `og:type`. Add Twitter Card tags.
4. **Generate sitemap** — create `sitemap.xml` with all public URLs, `lastmod` dates, and `changefreq` hints.
5. **Configure robots.txt** — allow crawling of public pages, block admin/API routes: `Disallow: /api/`.
6. **Validate** — test JSON-LD with Google Rich Results Test. Check OG tags with Facebook Sharing Debugger.
7. **Add canonical URLs** — `<link rel="canonical" href="...">` on every page to prevent duplicate content.

# JSON-LD examples

```html
<!-- Article -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "Article",
  "headline": "How to Build a Search-Friendly Site",
  "author": { "@type": "Person", "name": "Jane Doe" },
  "datePublished": "2024-06-15",
  "image": "https://example.com/hero.jpg"
}
</script>

<!-- FAQ -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "FAQPage",
  "mainEntity": [{
    "@type": "Question",
    "name": "What is JSON-LD?",
    "acceptedAnswer": { "@type": "Answer", "text": "A format for structured data in HTML." }
  }]
}
</script>
```

# Open Graph + Twitter meta

```html
<meta property="og:title" content="Page Title" />
<meta property="og:description" content="Brief description under 200 chars" />
<meta property="og:image" content="https://example.com/og-image.jpg" />
<meta property="og:url" content="https://example.com/page" />
<meta property="og:type" content="article" />
<meta name="twitter:card" content="summary_large_image" />
<meta name="twitter:title" content="Page Title" />
<meta name="twitter:description" content="Brief description" />
<meta name="twitter:image" content="https://example.com/og-image.jpg" />
<link rel="canonical" href="https://example.com/page" />
```

# Next.js metadata

```tsx
// app/blog/[slug]/page.tsx
export async function generateMetadata({ params }): Promise<Metadata> {
  const post = await getPost(params.slug);
  return {
    title: post.title,
    description: post.excerpt,
    openGraph: { title: post.title, images: [post.image] },
  };
}
```

# Decision rules

- JSON-LD over microdata or RDFa — easier to maintain, recommended by Google.
- OG image should be 1200x630px — optimal for all platforms.
- One canonical URL per page — prevents duplicate content penalties.
- Sitemap includes only indexable pages — no 404s, no `noindex` pages.
- Validate after every change — Google Rich Results Test catches errors immediately.

# References

- https://schema.org/
- https://developers.google.com/search/docs/appearance/structured-data
- https://ogp.me/

# Related skills

- `nextjs-app-router` — Next.js metadata API
- `frontend-performance` — performance impacts SEO
- `analytics-instrumentation` — tracking alongside SEO
