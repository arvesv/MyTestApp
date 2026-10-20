const fs = require('node:fs');
const path = require('node:path');

const target = process.argv[2] || 'patch';
const cleanTarget = target.trim().replace(/^["']|["']$/g, '');

console.log(`🚀 Bumping version with target: "${cleanTarget}"...`);

const csprojPath = path.resolve('src/MyTestApp/MyTestApp.csproj');
if (!fs.existsSync(csprojPath)) {
  console.error(`❌ Project file not found: ${csprojPath}`);
  process.exit(1);
}

let csprojContent = fs.readFileSync(csprojPath, 'utf8');
const versionMatch = csprojContent.match(/<Version>([^<]+)<\/Version>/);
const currentVersion = versionMatch ? versionMatch[1].trim() : '0.1.0';

console.log(`📦 Current version: ${currentVersion}`);

function bump(version, type) {
  const parts = version.replace(/^v/, '').split('.').map((p) => parseInt(p, 10));
  if (parts.length < 3 || parts.some(isNaN)) {
    throw new Error(`Cannot parse current version '${version}' as semver`);
  }
  let [major, minor, patch] = parts;
  if (type === 'major') {
    major += 1;
    minor = 0;
    patch = 0;
  } else if (type === 'minor') {
    minor += 1;
    patch = 0;
  } else if (type === 'patch') {
    patch += 1;
  } else {
    // Custom version provided
    const cleanCustom = type.replace(/^v/, '');
    if (!/^\d+\.\d+\.\d+/.test(cleanCustom)) {
      throw new Error(`Invalid version format: '${type}'. Expected major, minor, patch, or SemVer (e.g. 1.0.0)`);
    }
    return cleanCustom;
  }
  return `${major}.${minor}.${patch}`;
}

let rawVersion;
try {
  rawVersion = bump(currentVersion, cleanTarget);
} catch (err) {
  console.error(`❌ Failed to bump version:`, err.message);
  process.exit(1);
}

const tagVersion = `v${rawVersion}`;

// 1. Update src/MyTestApp/MyTestApp.csproj
if (csprojContent.includes('<Version>')) {
  csprojContent = csprojContent.replace(/<Version>[^<]+<\/Version>/, `<Version>${rawVersion}</Version>`);
} else {
  csprojContent = csprojContent.replace(/<\/PropertyGroup>/, `  <Version>${rawVersion}</Version>\n  </PropertyGroup>`);
}
fs.writeFileSync(csprojPath, csprojContent, 'utf8');
console.log(`✅ Updated MyTestApp.csproj <Version> to ${rawVersion}`);

// 2. Helper to update container image tags
function updateImageTag(filePath) {
  const resolvedPath = path.resolve(filePath);
  if (!fs.existsSync(resolvedPath)) {
    console.warn(`⚠️ Warning: ${filePath} does not exist, skipping.`);
    return;
  }
  const content = fs.readFileSync(resolvedPath, 'utf8');
  const regex = /(ghcr\.io\/[^/]+\/mytestapp:)[^\s'"]+/g;
  const updated = content.replace(regex, `$1${rawVersion}`);
  if (content !== updated) {
    fs.writeFileSync(resolvedPath, updated, 'utf8');
    console.log(`✅ Updated container image tag in ${filePath} -> :${rawVersion}`);
  } else {
    console.log(`ℹ️ No changes needed in ${filePath}`);
  }
}

// 3. Update compose.yaml and README.md
updateImageTag('compose.yaml');
updateImageTag('README.md');

// 4. Write GitHub Actions outputs if running in CI
if (process.env.GITHUB_OUTPUT) {
  fs.appendFileSync(process.env.GITHUB_OUTPUT, `version=${tagVersion}\n`);
  fs.appendFileSync(process.env.GITHUB_OUTPUT, `raw_version=${rawVersion}\n`);
}

console.log(`✨ Successfully bumped version to ${tagVersion} (${rawVersion})`);
