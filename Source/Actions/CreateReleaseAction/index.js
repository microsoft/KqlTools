const core = require('@actions/core');
const github = require('@actions/github');

const fs = require('fs');
const util = require('util');
const readdir = util.promisify(fs.readdir);

async function run()
{
	try
	{
		// set up octokit and context information
		const octokit = github.getOctokit(core.getInput('token'));

		// getting context
		const owner = github.context.repo.owner;
		const repo = github.context.repo.repo;

		// create new release
		const tag_name = core.getInput('tag_name');
		const release_name = core.getInput('release_name');
		const createReleaseResponse = await octokit.repos.createRelease({
			owner: owner,
			repo: repo,
			tag_name: tag_name,
			name: release_name
		});

		if(createReleaseResponse.status != 201)
		{
			core.setFailed(`Problem creating release: ${createReleaseResponse.status}`);
		}
		
		console.log("Release created!");

		// optionally upload release assets
		const dir = core.getInput('directory');
		if(!dir) return;
		console.log("Uploading asset(s) to release now!");

		const release = createReleaseResponse.data;
		const releaseAssets = await readdir(dir);

		for(let asset of releaseAssets)
		{
			console.log(`Uploading ${asset}...`);
			const uploadResponse = await octokit.repos.uploadReleaseAsset({
				owner: owner,
				repo: repo,
				release_id: release.id,
				name: asset,
				data: fs.readFileSync(`${dir}\\${asset}`),
				origin: release.upload_url
			});
			console.log(`Uploaded ${asset}!`);

			if(uploadResponse.status != 201)
			{
				core.setFailed(`Problem uploading release asset: ${asset}\nUpload response: ${uploadResponse.status}`);
			}
		}
	}
	catch(error)
	{
		core.setFailed(error.message);
	}
}

run();