const core = require('@actions/core');
const github = require('@actions/github');

const fs = require('fs');
const util = require('util');

const writefile = util.promisify(fs.writeFile);
const exec = util.promisify(require('child_process').exec);
const readdir = util.promisify(fs.readdir);

async function generatepfx(cert) {
  try {
    const secretcert = Buffer.from(core.getInput('certificate'), 'base64');
    await writefile(cert, secretcert);
    return true;
  }
  catch(err)
  {
    console.log(err);
    return false;
  }
}

async function getbinaries(dir) {
  try {
    const fileNames = await readdir(dir);
    let files = new Array(fileNames.length);
    await Promise.all(fileNames.map(async (fileName) => {
      const extension = fileName.split('.').pop();
      if(extension == 'exe') {
        files.push(`${dir}\\${fileName}`);
      }
    }))
    return files;
  }
  catch (err)
  {
    console.log(err);
    return null;
  }
}

async function sign(files, cert) {
  const key = core.getInput('key');
  try {
    await Promise.all(files.map(async (file) => {
      const { stdout, stderr } = await exec(`"C:\\Program Files (x86)\\Windows Kits\\10\\bin\\10.0.16299.0\\x64\\signtool.exe" sign /f "${cert}" /p ${key} /fd sha256 /tr "http://timestamp.digicert.com" /td sha256 "${file}"`);
      if(stderr) {
        console.error(`error: ${stderr}`);
      }
      else {
        console.log(stdout);
      }
    }));
    return true;
  }
  catch (err)
  {
    console.log(err);
    return false;
  }
}

async function run() {
  try {
    // prepare pfx for signtool
    const cert = `${process.env.temp}.pfx`;
    if (await generatepfx(cert)) {
    }

    // get binaries and sign
    const dir = core.getInput('directory');
    const files = await getbinaries(dir);
    if (await sign(files, cert))
    {
      console.log("Signed All Files!");
    }
  }
  catch (err) {
    core.setFailed(err);
  }
}

run();