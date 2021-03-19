const core = require('@actions/core');
const github = require('@actions/github');

const fs = require('fs');
const util = require('util');

async function run() {
  try {
    // creating __init__.py files
    const dir = core.getInput('directory');
    fs.writeFile(`${dir}\\__init__.py`, '', (err) => {
      if(err) {
        throw err;
      }
    });

    // generating setup.py files
    const tag = core.getInput('tag');
    const realtimekqlSetup = `import os
import glob
from setuptools import setup, find_packages

setup(
  name = 'realtimekql',
  version='${tag}',
  install_requires=['pythonnet'],
  packages=['.'],
  data_files=[('lib', glob.glob(os.path.join('lib', '*')))],
  include_package_data=True,
)`;
    
    fs.writeFile(`${dir}\\setup.py`, realtimekqlSetup, (err) => {
      if(err) {
        throw err;
      }
    });

  }
  catch (err) {
    core.setFailed(err);
  }
}

run();