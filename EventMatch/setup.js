#!/usr/bin/env node
// Setup script for EventMatch backend
// This prepares the backend for Render.com deployment

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const colors = {
    reset: '\x1b[0m',
    green: '\x1b[32m',
    yellow: '\x1b[33m',
    blue: '\x1b[36m',
    red: '\x1b[31m'
};

function log(msg, color = 'reset') {
    console.log(`${colors[color]}${msg}${colors.reset}`);
}

function logStep(step, desc) {
    log(`\n[${step}] ${desc}`, 'blue');
}

async function setup() {
    log('\n======================================', 'blue');
    log('EventMatch Backend Setup', 'blue');
    log('======================================\n', 'blue');

    try {
        // Step 1: Check if in correct directory
        logStep('1', 'Checking directory structure...');
        if (!fs.existsSync('BACKEND_EXAMPLE.js')) {
            throw new Error('BACKEND_EXAMPLE.js not found in current directory');
        }
        log('✓ Backend file found', 'green');

        // Step 2: Create package.json if doesn't exist
        logStep('2', 'Setting up package.json...');
        if (!fs.existsSync('package.json')) {
            const packageJson = {
                "name": "eventmatch-api",
                "version": "1.0.0",
                "description": "EventMatch API Server",
                "main": "BACKEND_EXAMPLE.js",
                "scripts": {
                    "start": "node BACKEND_EXAMPLE.js",
                    "dev": "nodemon BACKEND_EXAMPLE.js"
                },
                "keywords": [],
                "author": "",
                "license": "ISC",
                "dependencies": {
                    "express": "^4.18.2",
                    "cors": "^2.8.5",
                    "bcryptjs": "^2.4.3",
                    "sqlite3": "^5.1.6"
                },
                "devDependencies": {
                    "nodemon": "^3.0.1"
                }
            };
            fs.writeFileSync('package.json', JSON.stringify(packageJson, null, 2));
            log('✓ package.json created', 'green');
        } else {
            log('✓ package.json already exists', 'green');
        }

        // Step 3: Create .gitignore
        logStep('3', 'Creating .gitignore...');
        const gitignore = `node_modules/
*.db
.env
.env.local
.DS_Store
npm-debug.log
`;
        if (!fs.existsSync('.gitignore')) {
            fs.writeFileSync('.gitignore', gitignore);
            log('✓ .gitignore created', 'green');
        } else {
            log('✓ .gitignore already exists', 'green');
        }

        // Step 4: Create render.yaml
        logStep('4', 'Creating render.yaml...');
        const renderYaml = `services:
  - type: web
    name: eventmatch-api
    env: node
    plan: free
    buildCommand: npm install
    startCommand: node EventMatch/BACKEND_EXAMPLE.js
    envVars:
      - key: ADMIN_TOKEN
        value: jusu_secret
        scope: all
`;
        if (!fs.existsSync('render.yaml')) {
            fs.writeFileSync('render.yaml', renderYaml);
            log('✓ render.yaml created', 'green');
        } else {
            log('✓ render.yaml already exists', 'green');
        }

        // Step 5: Check npm
        logStep('5', 'Checking npm installation...');
        try {
            execSync('npm --version', { stdio: 'ignore' });
            log('✓ npm is installed', 'green');
        } catch {
            throw new Error('npm is not installed. Please install Node.js first.');
        }

        // Step 6: Install dependencies
        logStep('6', 'Installing dependencies...');
        log('This may take a few minutes...', 'yellow');
        execSync('npm install', { stdio: 'inherit' });
        log('✓ Dependencies installed', 'green');

        // Step 7: Create Render deployment guide
        logStep('7', 'Creating deployment guide...');
        const guide = `# Render.com Deployment Guide

## Quick Start:

1. Go to https://render.com
2. Sign up with GitHub
3. Create new Web Service
4. Select this repository
5. Configure:
   - Build Command: npm install
   - Start Command: node EventMatch/BACKEND_EXAMPLE.js
6. Add Environment Variables:
   - ADMIN_TOKEN = jusu_secret
7. Deploy!

Your API will be at: https://eventmatch-api.onrender.com

## After Deployment:

Test with:
\`\`\`powershell
curl https://eventmatch-api.onrender.com/api/health
\`\`\`

## To Update:

Just push to GitHub and Render will automatically redeploy!
\`\`\`bash
git push origin master
\`\`\`
`;
        fs.writeFileSync('RENDER_DEPLOYMENT.md', guide);
        log('✓ Deployment guide created', 'green');

        // Summary
        log('\n======================================', 'green');
        log('✓ SETUP COMPLETE!', 'green');
        log('======================================\n', 'green');

        log('Next steps:', 'blue');
        log('1. Commit to GitHub:', 'yellow');
        log('   git add .', 'yellow');
        log('   git commit -m "Setup for Render deployment"', 'yellow');
        log('   git push origin master', 'yellow');
        log('', 'reset');
        log('2. Go to https://render.com', 'yellow');
        log('3. Create Web Service from your repository', 'yellow');
        log('4. Configure start command: node EventMatch/BACKEND_EXAMPLE.js', 'yellow');
        log('5. Add ADMIN_TOKEN environment variable', 'yellow');
        log('6. Deploy!', 'yellow');
        log('', 'reset');
        log('API will be available at: https://eventmatch-api.onrender.com', 'green');

    } catch (error) {
        log(`\n✗ Error: ${error.message}`, 'red');
        process.exit(1);
    }
}

setup();
