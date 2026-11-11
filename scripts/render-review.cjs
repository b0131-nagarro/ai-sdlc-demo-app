const fs = require('node:fs');
const d = JSON.parse(fs.readFileSync(process.argv[2], 'utf8'));
const esc = (s) => String(s === undefined || s === null ? '' : s).replace(/\|/g, '\\|').replace(/\n/g, ' ');
console.log('| ID | Severity | Location | Finding | Verdict | Reason |');
console.log('|---|---|---|---|---|---|');
for (const x of d.findings) {
console.log('| ' + x.id + ' | ' + x.severity + ' | ' + esc(x.file) + ':' + x.line + ' | ' + esc(x.finding) +
' | ' + esc(x.verdict || 'TBD') + ' | ' + esc(x.verdict_reason) + ' |');
}