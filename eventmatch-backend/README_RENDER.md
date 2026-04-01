Deploy EventMatch backend to Render.com

1) Create a new "Web Service" on Render using your GitHub repo.
   - Connect your GitHub account.
   - Select the repository and branch with the backend code (e.g., `eventmatch-backend`).

2) Build & Start Commands
   - Build command: leave blank (no build step required)
   - Start command: `npm install && npm start`

3) Environment
   - Set the following environment variables in Render (Dashboard -> Service -> Environment):
     - `NODE_ENV` = `production`
     - (optional) `EVENTMATCH_DB_PATH` = `/var/data/eventmatch.db`  # makes DB persistent across restarts if using a persistent disk

4) Disk
   - For data persistence, enable a persistent disk in Render and set `EVENTMATCH_DB_PATH` to the mounted path (e.g., `/var/data/eventmatch.db`).

5) Health check
   - Render will call the service root. The app exposes `/api/health` for status.

6) Troubleshooting
   - Check service logs in Render dashboard if any failures.
   - Ensure `package.json` is present (we use `BACKEND_PACKAGE.json` — rename to `package.json` or copy contents to `package.json` in repo root).

7) Notes
   - If you prefer to use SQLite only for development, consider moving to a managed DB for production.

That's it — push your branch and create the Render service. Once deployed, update your mobile app `EVENTMATCH_API_BASE_URL` if needed to point to the Render service URL.
