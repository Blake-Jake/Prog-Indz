// Add these routes to your BACKEND_EXAMPLE.js Node.js server
// These handle group synchronization between emulators

/**
 * ============= GROUP ROUTES =============
 * These endpoints should be added to your Express app
 */

// POST /api/groups/create - Create a new group
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

// GET /api/groups/user/:email - Get all groups for a user
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

// GET /api/groups/:id - Get specific group
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

// PUT /api/groups/:id - Update group
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

// DELETE /api/groups/:id - Delete group and all related data
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
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// POST /api/groups/add-member - Add user to group
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

// POST /api/groups/messages - Add message to group
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

// GET /api/groups/:id/messages - Get messages for group
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

// GET /api/groups/:id/members - Get members of group
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

// ============= DATABASE SCHEMA =============
// Make sure these tables exist in your database:

// CREATE TABLE groups (
//     id INTEGER PRIMARY KEY AUTOINCREMENT,
//     name TEXT NOT NULL,
//     description TEXT,
//     ownerEmail TEXT NOT NULL,
//     memberCount INTEGER DEFAULT 1,
//     createdAt DATETIME DEFAULT CURRENT_TIMESTAMP
// );

// CREATE TABLE groupMembers (
//     id INTEGER PRIMARY KEY AUTOINCREMENT,
//     groupId INTEGER NOT NULL,
//     userEmail TEXT NOT NULL,
//     joinedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
//     FOREIGN KEY (groupId) REFERENCES groups(id)
// );

// CREATE TABLE groupMessages (
//     id INTEGER PRIMARY KEY AUTOINCREMENT,
//     groupId INTEGER NOT NULL,
//     fromEmail TEXT NOT NULL,
//     text TEXT NOT NULL,
//     timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
//     FOREIGN KEY (groupId) REFERENCES groups(id)
// );

// ============= INITIALIZATION =============
// Add this to your database initialization section:

db.serialize(() => {
    db.run(`CREATE TABLE IF NOT EXISTS groups (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        name TEXT NOT NULL,
        description TEXT,
        ownerEmail TEXT NOT NULL,
        memberCount INTEGER DEFAULT 1,
        createdAt DATETIME DEFAULT CURRENT_TIMESTAMP
    )`);

    db.run(`CREATE TABLE IF NOT EXISTS groupMembers (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        groupId INTEGER NOT NULL,
        userEmail TEXT NOT NULL,
        joinedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (groupId) REFERENCES groups(id)
    )`);

    db.run(`CREATE TABLE IF NOT EXISTS groupMessages (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        groupId INTEGER NOT NULL,
        fromEmail TEXT NOT NULL,
        text TEXT NOT NULL,
        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (groupId) REFERENCES groups(id)
    )`);

    console.log('Group tables created/verified');
});
