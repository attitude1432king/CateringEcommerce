/**
 * migrate-color-literals.mjs
 *
 * Usage:
 *   node scripts/migrate-color-literals.mjs           # preview diff only (safe)
 *   node scripts/migrate-color-literals.mjs --apply   # write changes to disk
 *
 * Run once, review the diff, then delete this script.
 *
 * SKIPPED intentionally:
 *   - src/design-system/tokens.css    (source of truth — must stay as hex)
 *   - src/design-system/enyvora.css   (explicit Preflight-bypass sub-properties must stay as hex)
 *   - AppHeader.jsx                   (SVG data URLs — var() doesn't resolve inside base64 SVGs)
 */

import { readFileSync, writeFileSync, readdirSync } from 'fs';
import { resolve, relative } from 'path';
import { fileURLToPath } from 'url';
import { dirname } from 'path';

const __dirname = dirname(fileURLToPath(import.meta.url));
const SRC = resolve(__dirname, '../src');

// ── files to skip entirely ───────────────────────────────────────────────────

const SKIP_FRAGMENTS = [
    'design-system/tokens.css',
    'design-system/enyvora.css',
    'Header/AppHeader.jsx',
];

function shouldSkip(filePath) {
    const rel = relative(SRC, filePath).replace(/\\/g, '/');
    return SKIP_FRAGMENTS.some(s => rel.includes(s));
}

// ── replacement rules ────────────────────────────────────────────────────────

// CSS: replace bare hex literals with token var()
const CSS_RULES = [
    { from: /#FF6B35(?![0-9a-fA-F])/g, to: 'var(--color-primary)' },
    { from: /#E55A24(?![0-9a-fA-F])/g, to: 'var(--color-primary-dark)' },
    { from: /#FF8C42(?![0-9a-fA-F])/g, to: 'var(--color-secondary)' },
    { from: /#FFB627(?![0-9a-fA-F])/g, to: 'var(--color-accent)' },
    { from: /#FFF8F3(?![0-9a-fA-F])/g, to: 'var(--color-light)' },
];

// JSX: Tailwind arbitrary-value class strings → token utilities
// Does NOT touch inline-style gradient strings (those are intentional Preflight bypasses).
const JSX_RULES = [
    { from: /text-\[#FF6B35\]/g,   to: 'text-primary' },
    { from: /bg-\[#FF6B35\]/g,     to: 'bg-primary' },
    { from: /border-\[#FF6B35\]/g, to: 'border-primary' },
    { from: /text-\[#FFB627\]/g,   to: 'text-accent' },
    { from: /bg-\[#FFB627\]/g,     to: 'bg-accent' },
    { from: /border-\[#FFB627\]/g, to: 'border-accent' },
];

// ── recursive file walker ────────────────────────────────────────────────────

function walkDirSync(dir) {
    const files = [];
    for (const entry of readdirSync(dir, { withFileTypes: true })) {
        const full = resolve(dir, entry.name);
        if (entry.isDirectory()) {
            files.push(...walkDirSync(full));
        } else if (/\.(jsx?|css)$/.test(entry.name)) {
            files.push(full);
        }
    }
    return files;
}

// ── main ─────────────────────────────────────────────────────────────────────

const apply = process.argv.includes('--apply');
const changes = [];

for (const filePath of walkDirSync(SRC)) {
    if (shouldSkip(filePath)) continue;

    const original = readFileSync(filePath, 'utf8');
    const rules = filePath.endsWith('.css') ? CSS_RULES : JSX_RULES;

    let result = original;
    for (const { from, to } of rules) result = result.replace(from, to);

    if (result !== original) {
        changes.push({ rel: relative(SRC, filePath).replace(/\\/g, '/'), original, result, filePath });
    }
}

// ── report ────────────────────────────────────────────────────────────────────

if (changes.length === 0) {
    console.log('No replaceable color literals found outside skipped files.');
    process.exit(0);
}

console.log(`\nFound ${changes.length} file(s) with replaceable literals:\n`);
for (const { rel, original, result } of changes) {
    console.log(`  ${rel}`);
    const orig = original.split('\n');
    const next = result.split('\n');
    for (let i = 0; i < orig.length; i++) {
        if (orig[i] !== next[i]) {
            console.log(`    L${i + 1}  - ${orig[i].trim()}`);
            console.log(`    L${i + 1}  + ${next[i].trim()}`);
        }
    }
    console.log();
}

if (!apply) {
    console.log('DRY RUN — no files written. Re-run with --apply to apply changes.');
} else {
    for (const { filePath, result } of changes) writeFileSync(filePath, result, 'utf8');
    console.log(`Applied to ${changes.length} file(s). Review with git diff, then delete this script.`);
}
