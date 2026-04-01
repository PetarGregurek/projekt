# AI Chat Logging Instructions

This workspace is configured to automatically log all AI interactions for educational documentation and auditing purposes.

## Automatic Logging

The following interactions are automatically logged to `.github/hooks/agent_log.txt`:
- **SessionStart**: When you start a new chat session
- **UserPromptSubmit**: Every message you submit
- **PreToolUse**: When a tool is about to be used
- **PostToolUse**: When a tool execution completes
- **Stop**: When the session ends

## Manual Logging Command

If automatic logging isn't working, use this manual command to log messages:

```powershell
$logFile = ".github/hooks/agent_log.txt"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$message = "Your message here"
Add-Content -Path $logFile -Value "[$timestamp] MANUAL: $message" -Encoding UTF8
```

## Log File Location

**Path**: `.github/hooks/agent_log.txt`

**Log Format**:
```
[2026-04-01 14:30:25] SESSION_START
[2026-04-01 14:30:26] USER: Help me fix this error
[2026-04-01 14:30:27] TOOL_CALLED: read_file
[2026-04-01 14:30:28] TOOL_RESULT: Success
[2026-04-01 14:30:29] SESSION_END
```

## Educational Use

This log serves as an audit trail documenting:
- How AI assistants were used in development
- What tools and features were leveraged
- Timeline of interactions during the development process

Perfect for assignment submissions and demonstrating AI usage in educational projects.
