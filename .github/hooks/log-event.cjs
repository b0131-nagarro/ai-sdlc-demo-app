const fs = require('node:fs');
const path = require('node:path');
const eventName = process.argv[2] || 'unknown';
let raw = '';
try { raw = fs.readFileSync(0, 'utf8'); } catch (e) {}
let p = {};
try { p = JSON.parse(raw || '{}'); } catch (e) {}
const redact = (s) => String(s === undefined || s === null ? '' : s)
.replace(/(ghp_|gho_|github_pat_|sk-)[A-Za-z0-9_-]+/g, '[REDACTED]')
.slice(0, 1500);
const args = p.toolArgs !== undefined ? p.toolArgs : p.tool_input;
const entry = {
ts: new Date().toISOString(),
event: eventName,
session: p.sessionId || p.session_id || null,
tool: p.toolName || p.tool_name,
tool_args: args !== undefined ? redact(JSON.stringify(args)) : undefined,
prompt: p.prompt !== undefined ? redact(p.prompt) : undefined,
reason: p.reason,
payload_keys: Object.keys(p),
raw: eventName.startsWith('subagent') ? redact(raw) : undefined,
};
const dir = path.join(__dirname, '..', '..', 'docs', 'ai-log');
fs.mkdirSync(dir, { recursive: true });
fs.appendFileSync(path.join(dir, 'ai-interactions.jsonl'), JSON.stringify(entry) + '\n');
process.stdout.write('{}');