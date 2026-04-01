// Backend API for EventMatch - Node.js Express Server
// Deploy to Render.com or Heroku for production
// This backend handles both user authentication AND group synchronization

const express = require('express');
const cors = require('cors');
const bcrypt = require('bcryptjs');
const sqlite3 = require('sqlite3').verbose();
const path = require('path');

const app = express();
const PORT = process.env.PORT || 5000;

// Middleware
app.use(cors());
app.use(express.json());

// Initialize SQLite Database
const dbPath = path.join(__dirname, 'eventmatch.db');
const db = new sqlite3.Database(dbPath, (err) => {
    if (err) console.error('Database error:', err);
    else console.log('Connected to SQLite database');
});

// ============= CREATE TABLES =============
// Initialize all tables for users and groups
db.serialize(() => {
    // Users table
    db.run(`
        CREATE TABLE IF NOT EXISTS users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            email TEXT UNIQUE NOT NULL,
            password TEXT NOT NULL,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP
        )
    `);

    // Groups table
    db.run(`
        CREATE TABLE IF NOT EXISTS groups (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            description TEXT,
            ownerEmail TEXT NOT NULL,
            memberCount INTEGER DEFAULT 1,
            createdAt DATETIME DEFAULT CURRENT_TIMESTAMP
        )
    `);

    // Group members table
    db.run(`
        CREATE TABLE IF NOT EXISTS groupMembers (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            groupId INTEGER NOT NULL,
            userEmail TEXT NOT NULL,
            joinedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (groupId) REFERENCES groups(id)
        )
    `);

    // Group messages table
    db.run(`
        CREATE TABLE IF NOT EXISTS groupMessages (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            groupId INTEGER NOT NULL,
            fromEmail TEXT NOT NULL,
            text TEXT NOT NULL,
            timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (groupId) REFERENCES groups(id)
        )
    `);

    console.log('All tables created/verified');
});

// ============= AUTH ROUTES =============

/**
 * POST /api/auth/register
 * Register a new user
 */
app.post('/api/auth/register', async (req, res) => {
    try {
        const { email, password } = req.body;

        if (!email || !password) {
            return res.status(400).json({ error: 'Email and password required' });
        }

        // Hash password
        const hashedPassword = await bcrypt.hash(password, 10);

        // Insert user
        db.run(
            'INSERT INTO users (email, password) VALUES (?, ?)',
            [email, hashedPassword],
            (err) => {
                if (err) {
                    if (err.message.includes('UNIQUE constraint failed')) {
                        return res.status(409).json({ error: 'Email already exists' });
                    }
                    return res.status(500).json({ error: 'Registration failed' });
                }
                res.status(201).json({ 
                    message: 'User registered successfully',
                    email: email
                });
            }
        );
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

/**
 * POST /api/auth/login
 * Authenticate user and return user object
 */
app.post('/api/auth/login', async (req, res) => {
    try {
        const { email, password } = req.body;

        if (!email || !password) {
            return res.status(400).json({ error: 'Email and password required' });
        }

        // Find user
        db.get(
            'SELECT id, email, password FROM users WHERE email = ?',
            [email],
            async (err, user) => {
                if (err) {
                    return res.status(500).json({ error: 'Database error' });
                }

                if (!user) {
                    return res.status(401).json({ error: 'Invalid credentials' });
                }

                // Verify password
                const validPassword = await bcrypt.compare(password, user.password);
                if (!validPassword) {
                    return res.status(401).json({ error: 'Invalid credentials' });
                }

                // Return user object (matching the C# User model)
                res.json({
                    id: user.id,
                    email: user.email,
                    password: password // Return plain password (already validated)
                });
            }
        );
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

/**
 * GET /api/auth/exists/:email
 * Check if user exists
 */
app.get('/api/auth/exists/:email', (req, res) => {
    try {
        const { email } = req.params;

        db.get(
            'SELECT email FROM users WHERE email = ?',
            [email],
            (err, user) => {
                if (err) {
                    return res.status(500).json({ error: 'Database error' });
                }

                if (user) {
                    res.status(200).json({ exists: true });
                } else {
                    res.status(404).json({ exists: false });
                }
            }
        );
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// ============= GROUP ROUTES =============

/**
 * POST /api/groups/create
 * Create a new group
 */
app.post('/api/groups/create', (req, res) => {
    try {
        const { name, description, ownerEmail, memberCount } = req.body;

        if (!name || !ownerEmail) {
            return res.status(400).json({ error: 'Name and ownerEmail required' });
        }

        const groupData = {
            name,
            description: description || '',
            ownerEmail,
            memberCount: memberCount || 1,
            createdAt: new Date()
        };

        db.run(
            'INSERT INTO groups (name, description, ownerEmail, memberCount, createdAt) VALUES (?, ?, ?, ?, ?)',
            [groupData.name, groupData.description, groupData.ownerEmail, groupData.memberCount, groupData.createdAt],
            function(err) {
                if (err) {
                    return res.status(500).json({ error: 'Failed to create group' });
                }
                res.status(201).json({
                    id: this.lastID,
                    ...groupData
                });
            }
        );
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

/**
 * GET /api/groups/user/:email
 * Get all groups for a user
 */
app.get('/api/groups/user/:email', (req, res) => {
    try {
        const { email } = req.params;

        db.all(
            `SELECT DISTINCT g.* FROM groups g
             LEFT JOIN groupMembers gm ON g.id = gm.groupId
             WHERE g.ownerEmail = ? OR gm.userEmail = ?
             ORDER BY g.createdAt DESC`,
            [email, email],
            (err, groups) => {
                if (err) {
                    return res.status(500).json({ error: 'Database error' });
                }
                res.json(groups || []);
            }
        );
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

/**
 * GET /api/groups/:id
 * Get specific group
 */
app.get('/api/groups/:id', (req, res) => {
    try {
        const { id } = req.params;

        db.get(
            'SELECT * FROM groups WHERE id = ?',
            [id],
            (err, group) => {
                if (err) {
                    return res.status(500).json({ error: 'Database error' });
                }
                if (!group) {
                    return res.status(404).json({ error: 'Group not found' });
                }
                res.json(group);
            }
        );
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

/**
 * PUT /api/groups/:id
 * Update group
 */
app.put('/api/groups/:id', (req, res) => {
    try {
        const { id } = req.params;
        const { name, description, memberCount } = req.body;

        db.run(
            'UPDATE groups SET name = ?, description = ?, memberCount = ? WHERE id = ?',
            [name, description, memberCount, id],
            (err) => {
                if (err) {
                    return res.status(500).json({ error: 'Failed to update group' });
                }
                res.json({ message: 'Group updated', id });
            }
        );
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

/**
 * DELETE /api/groups/:id
 * Delete group and all related data
 */
app.delete('/api/groups/:id', (req, res) => {
    try {
        const { id } = req.params;

        // Delete in transaction
        db.serialize(() => {
            // Delete messages
            db.run('DELETE FROM groupMessages WHERE groupId = ?', [id]);
            // Delete members
            db.run('DELETE FROM groupMembers WHERE groupId = ?', [id]);
            // Delete group
            db.run('DELETE FROM groups WHERE id = ?', [id], (err) => {
                if (err) {
                    return res.status(500).json({ error: 'Failed to delete group' });
                }
                res.json({ message: 'Group deleted' });
            });
        });

/**
 * POST /api/admin/delete-all
 * Destructive admin endpoint to wipe all users, groups, members and messages.
 * Requires header 'x-admin-token' matching the server's ADMIN_TOKEN environment variable.
 */
app.post('/api/admin/delete-all', (req, res) => {
    try {
        const provided = req.headers['x-admin-token'];
        const adminToken = process.env.ADMIN_TOKEN;

        if (!adminToken) {
            console.error('Admin delete attempted but ADMIN_TOKEN is not set on server');
            return res.status(500).json({ error: 'Admin token not configured on server' });
        }

        if (!provided || provided !== adminToken) {
            return res.status(403).json({ error: 'Forbidden' });
        }

        db.serialize(() => {
            db.run('DELETE FROM groupMessages', (err) => { if (err) console.error('delete groupMessages', err); });
            db.run('DELETE FROM groupMembers', (err) => { if (err) console.error('delete groupMembers', err); });
            db.run('DELETE FROM groups', (err) => { if (err) console.error('delete groups', err); });
            db.run('DELETE FROM users', (err) => {
                if (err) {
                    console.error('delete users', err);
                    return res.status(500).json({ error: 'Failed to delete data' });
                }
                // Optionally vacuum to reclaim space
                db.run('VACUUM', (vErr) => { if (vErr) console.error('vacuum', vErr); });
                res.json({ message: 'All data deleted' });
            });
        });
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

/**
 * POST /api/groups/add-member
 * Add user to group
 */
app.post('/api/groups/add-member', (req, res) => {
    try {
        const { groupId, userEmail } = req.body;

        if (!groupId || !userEmail) {
            return res.status(400).json({ error: 'GroupId and userEmail required' });
        }

        // Check if already member
        db.get(
            'SELECT * FROM groupMembers WHERE groupId = ? AND userEmail = ?',
            [groupId, userEmail],
            (err, row) => {
                if (row) {
                    return res.status(400).json({ error: 'User already member' });
                }

                db.run(
                    'INSERT INTO groupMembers (groupId, userEmail) VALUES (?, ?)',
                    [groupId, userEmail],
                    (err) => {
                        if (err) {
                            return res.status(500).json({ error: 'Failed to add member' });
                        }

                        // Increment member count
                        db.run(
                            'UPDATE groups SET memberCount = memberCount + 1 WHERE id = ?',
                            [groupId],
                            (err) => {
                                if (err) console.error(err);
                                res.json({ message: 'Member added' });
                            }
                        );
                    }
                );
            }
        );
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

/**
 * POST /api/groups/messages
 * Add message to group
 */
app.post('/api/groups/messages', (req, res) => {
    try {
        const { groupId, fromEmail, text } = req.body;

        if (!groupId || !fromEmail || !text) {
            return res.status(400).json({ error: 'GroupId, fromEmail, and text required' });
        }

        const messageData = {
            groupId,
            fromEmail,
            text,
            timestamp: new Date()
        };

        db.run(
            'INSERT INTO groupMessages (groupId, fromEmail, text, timestamp) VALUES (?, ?, ?, ?)',
            [messageData.groupId, messageData.fromEmail, messageData.text, messageData.timestamp],
            function(err) {
                if (err) {
                    return res.status(500).json({ error: 'Failed to add message' });
                }
                res.status(201).json({
                    id: this.lastID,
                    ...messageData
                });
            }
        );
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

/**
 * GET /api/groups/:id/messages
 * Get messages for group
 */
app.get('/api/groups/:id/messages', (req, res) => {
    try {
        const { id } = req.params;

        db.all(
            'SELECT * FROM groupMessages WHERE groupId = ? ORDER BY timestamp ASC',
            [id],
            (err, messages) => {
                if (err) {
                    return res.status(500).json({ error: 'Database error' });
                }
                res.json(messages || []);
            }
        );
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

/**
 * GET /api/groups/:id/members
 * Get members of group
 */
app.get('/api/groups/:id/members', (req, res) => {
    try {
        const { id } = req.params;

        db.all(
            'SELECT userEmail FROM groupMembers WHERE groupId = ?',
            [id],
            (err, members) => {
                if (err) {
                    return res.status(500).json({ error: 'Database error' });
                }
                res.json(members || []);
            }
        );
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// ============= STATUS & ADMIN ROUTES =============

/**
 * GET /api/users
 * Get all users from database (admin endpoint)
 */
app.get('/api/users', (req, res) => {
    try {
        db.all('SELECT id, email, created_at FROM users', (err, users) => {
            if (err) {
                return res.status(500).json({ error: 'Database error' });
            }
            res.json(users || []);
        });
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

/**
 * GET /api/groups
 * Get all groups from database (public browse)
 */
app.get('/api/groups', (req, res) => {
    try {
        db.all('SELECT * FROM groups ORDER BY createdAt DESC', (err, groups) => {
            if (err) {
                return res.status(500).json({ error: 'Database error' });
            }
            res.json(groups || []);
        });
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// ============= HEALTH & STATUS =============

/**
 * GET /api/health
 * Health check endpoint
 */
app.get('/api/health', (req, res) => {
    res.json({ 
        status: 'OK', 
        message: 'Server is running',
        endpoints: {
            auth: ['/api/auth/register', '/api/auth/login', '/api/auth/exists/:email'],
            groups: [
                '/api/groups/create',
                '/api/groups/user/:email',
                '/api/groups/:id',
                '/api/groups/:id (PUT)',
                '/api/groups/:id (DELETE)',
                '/api/groups/add-member',
                '/api/groups/:id/messages',
                '/api/groups/messages',
                '/api/groups/:id/members'
            ]
        }
    });
});

// ============= START SERVER =============
app.listen(PORT, () => {
    console.log(`🚀 EventMatch API Server running on port ${PORT}`);
    console.log(`📍 Local: http://localhost:${PORT}`);
    console.log(`💚 Health Check: http://localhost:${PORT}/api/health`);
    console.log(`\n✨ Features:`);
    console.log(`  ✅ User Authentication (register, login)`);
    console.log(`  ✅ Group Management (create, edit, delete)`);
    console.log(`  ✅ Group Messaging (send, retrieve)`);
    console.log(`  ✅ Group Membership (add members)`);
});

// Handle graceful shutdown
process.on('SIGINT', () => {
    db.close((err) => {
        if (err) console.error('Error closing database:', err);
        console.log('Database closed. Server shutting down...');
        process.exit(0);
    });
});
