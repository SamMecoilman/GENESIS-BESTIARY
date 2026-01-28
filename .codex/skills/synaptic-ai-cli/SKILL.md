# synaptic-ai-cli (project-local)

Use this skill when the user asks to control the Unity project via Synaptic AI Pro's HTTP server (e.g., mentions "Synaptic AI Pro", "Unity HTTP", "Synaptic CLI", or asks to call the /execute or /batch endpoints).

## Prerequisites
- Unity is open with the GENESIS-BESTIARY project loaded.
- Synaptic AI Pro HTTP Server is running (Tools > Synaptic Pro > Synaptic Setup > HTTP Server tab) or Auto-Start on Load is enabled.

## Connection check (REQUIRED first)
Run this EXACTLY:

curl http://localhost:8086/health

If connection fails (e.g., refused), tell the user to start the server in Unity as noted above. If curl fails due to sandbox restrictions, rerun with escalation.

## Tool discovery (optional)
Run these EXACTLY when needed:

curl http://localhost:8086/categories
curl http://localhost:8086/tools/category/scene

## Execute tools (preferred)
Single tool (RECOMMENDED). Run EXACTLY:

curl -X POST http://localhost:8086/execute -H "Content-Type: application/json" -d '{"tool":"unity_create_gameobject","params":{"name":"MyCube","type":"cube"}}'

Batch execution (RECOMMENDED for multiple operations). Run EXACTLY:

curl -X POST http://localhost:8086/batch -H "Content-Type: application/json" -d '[
  {"tool":"unity_create_gameobject","params":{"name":"Cube1","type":"cube"}},
  {"tool":"unity_set_transform","params":{"name":"Cube1","position":"2,0,0"}},
  {"tool":"unity_create_gameobject","params":{"name":"Sphere1","type":"sphere"}}
]'

## Notes
- All responses are JSON.
- Timeout: 30 seconds per request.
- Do not modify the curl commands (no extra flags, redirects, or wrappers).
- If you see sandbox errors (e.g., localhost blocked), rerun the curl command with escalation.
