const express = require('express');
const fs = require('fs').promises;
const path = require('path');

const app = express();
app.use(express.json());

const VAULT_PATH = './obsidian-vault';

// Ensure vault directory exists
async function ensureVaultExists() {
    try {
        await fs.mkdir(VAULT_PATH, { recursive: true });
    } catch (error) {
        console.error('Error creating vault directory:', error);
    }
}

// Sanitize filename for filesystem
function sanitizeFilename(title) {
    return title.replace(/[^a-z0-9]/gi, ' ')
        .trim()
        .replace(/\s+/g, '-')
        .toLowerCase();
}

// Create a single page with its link
async function createPage(title, linkedPage, relationship) {
    const filename = sanitizeFilename(title);
    const filePath = path.join(VAULT_PATH, `${filename}.md`);
    
    const content = `# ${title}\n\n## Links\n- [[${linkedPage}]] - ${relationship}\n`;
    
    await fs.writeFile(filePath, content, 'utf8');
    return filePath;
}

// Generate multiple pages from a starting point
async function generatePages(startPage, relationship, depth = 5) {
    const pages = new Set();
    let currentPage = startPage;
    
    for (let i = 0; i < depth; i++) {
        const nextPage = `Page-${i + 1}`;
        await createPage(currentPage, nextPage, relationship);
        pages.add(currentPage);
        currentPage = nextPage;
    }
    
    // Create the final page with a link back to start
    await createPage(currentPage, startPage, relationship);
    pages.add(currentPage);
    
    return Array.from(pages);
}

// API Routes
app.post('/api/generate', async (req, res) => {
    try {
        const { startPage, relationship, depth } = req.body;
        
        if (!startPage || !relationship) {
            return res.status(400).json({ 
                error: 'Missing required fields: startPage and relationship' 
            });
        }
        
        await ensureVaultExists();
        const generatedPages = await generatePages(
            startPage, 
            relationship, 
            depth || 5
        );
        
        res.json({
            success: true,
            pages: generatedPages,
            vaultPath: VAULT_PATH
        });
    } catch (error) {
        console.error('Error generating pages:', error);
        res.status(500).json({ 
            error: 'Failed to generate pages',
            details: error.message 
        });
    }
});

// Get list of all generated pages
app.get('/api/pages', async (req, res) => {
    try {
        const files = await fs.readdir(VAULT_PATH);
        const pages = files
            .filter(file => file.endsWith('.md'))
            .map(file => file.replace('.md', ''));
        
        res.json({ pages });
    } catch (error) {
        res.status(500).json({ 
            error: 'Failed to list pages',
            details: error.message 
        });
    }
});

// Delete all generated pages
app.delete('/api/pages', async (req, res) => {
    try {
        const files = await fs.readdir(VAULT_PATH);
        await Promise.all(
            files.map(file => 
                fs.unlink(path.join(VAULT_PATH, file))
            )
        );
        
        res.json({ 
            success: true, 
            message: 'All pages deleted' 
        });
    } catch (error) {
        res.status(500).json({ 
            error: 'Failed to delete pages',
            details: error.message 
        });
    }
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});