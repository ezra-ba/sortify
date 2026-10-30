// Synthetic responses only; never forwards requests to NVIDIA.
const http = require('http');
http.createServer(async (req, res) => {
  if (req.method !== 'POST' || req.url !== '/v1/chat/completions') {
    res.writeHead(501); return res.end();
  }
  let raw = '';
  for await (const chunk of req) raw += chunk;
  try {
    const body = JSON.parse(raw);
    if (req.headers.authorization !== 'Bearer sortify-test-only' ||
        body.messages.length !== 8 || body.stream !== false ||
        body.chat_template_kwargs.enable_thinking !== false) {
      res.writeHead(400); return res.end();
    }
    const context = JSON.parse(body.messages.at(-1).content);
    const name = context.file.name;
    const status = { 'unauthorized.pdf': 401, 'limited.pdf': 429, 'server.pdf': 500 }[name];
    if (status) { res.writeHead(status); return res.end(); }
    if (name === 'timeout.pdf' || name === 'cancel.pdf')
      await new Promise(resolve => setTimeout(resolve, 1000));
    let content = JSON.stringify({ status: 'suggested', targetFolderId: context.targets[0].id,
      proposedFileName: name, reason: 'Semantischer Testvorschlag.', appliedRuleIds: [] });
    if (name === 'invalid.pdf') content = 'not json';
    if (name === 'large.pdf') content = 'x'.repeat(70000);
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ choices: [{ finish_reason: name === 'truncated.pdf' ? 'length' : 'stop', message: { content } }] }));
  } catch { res.writeHead(400); res.end(); }
}).listen(process.env.PORT || 4500);
