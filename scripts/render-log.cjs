const fs = require('node:fs');
const file = 'docs/ai-log/ai-interactions.jsonl';
const rows = fs.readFileSync(file, 'utf8').split('\n').filter(Boolean).map((l) => JSON.parse(l));
const esc = (s) => String(s === undefined ? '' : s).replace(/\|/g, '\\|').replace(/\n/g, ' ').slice(0, 160);
console.log('# AI interaction log (rendered from ai-interactions.jsonl)\n');
console.log('| Time (UTC) | Event | Tool | Prompt or tool arguments |');
console.log('|---|---|---|---|');
for (const r of rows) {
const detail = esc(r.prompt || r.tool_args);
console.log('| ' + r.ts + ' | ' + r.event + ' | ' + (r.tool || '') + ' | ' + detail + ' |');
}