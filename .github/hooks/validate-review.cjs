const fs = require('node:fs');
const path = require('node:path');
try { fs.readFileSync(0, 'utf8'); } catch (e) {}
const dir = path.join(__dirname, '..', '..', 'docs', 'reviews');
const SEVERITIES = ['blocker', 'major', 'minor', 'nit'];
const REQUIRED = ['id', 'severity', 'file', 'line', 'finding', 'suggested_fix', 'verdict', 'verdict_reason'];
const problems = [];
if (fs.existsSync(dir)) {
for (const f of fs.readdirSync(dir).filter((x) => x.endsWith('.json'))) {
let doc;
try { doc = JSON.parse(fs.readFileSync(path.join(dir, f), 'utf8')); }
catch (e) { problems.push(f + ': not valid JSON'); continue; }
if (!Array.isArray(doc.findings)) { problems.push(f + ': missing "findings" array'); continue; }
doc.findings.forEach((x, i) => {
for (const k of REQUIRED) {
if (!(k in x)) problems.push(f + ' finding #' + (i + 1) + ': missing "' + k + '"');
}
if (x.severity && !SEVERITIES.includes(x.severity)) {
problems.push(f + ' finding #' + (i + 1) + ': severity must be ' + SEVERITIES.join('/'));
}
});
}
}
const msg = 'REVIEW FORMAT ERRORS, fix before finishing: ' + problems.join('; ');
process.stdout.write(problems.length ? JSON.stringify({ additionalContext: msg }) : '{}');