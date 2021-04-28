import os
import glob
from setuptools import setup, find_packages

with open(os.path.join(os.path.abspath(os.path.dirname(__file__)), 'VERSION.txt'), encoding='utf-8') as f:
	version = f.read()

with open(os.path.join(os.path.abspath(os.path.dirname(__file__)), 'README.md'), encoding='utf-8') as f:
	long_description = f.read()

setup(
	name = 'realtimekql',
	version=version,
	author='CDOC Engineering Open Source',
	author_email='CDOCEngOpenSourceAdm@microsoft.com',
	url='https://github.com/microsoft/kqltools',
	description='A module for exploring real-time streams of events',
	long_description=long_description,
	long_description_content_type='text/markdown',
	install_requires=['pythonnet', 'pandas', 'azure.kusto.data', 'azure.kusto.ingest'],
	packages=['.'],
	data_files=[('lib', glob.glob(os.path.join('lib', '*')))],
	include_package_data=True,
	classifiers=[
		'Environment :: Console',
		'Intended Audience :: End Users/Desktop',
		'Intended Audience :: Developers',
		'License :: OSI Approved :: Apache Software License',
		'Natural Language :: English',
		'Operating System :: Microsoft :: Windows',
		'Programming Language :: C#',
		'Programming Language :: Python :: 3',
		'Topic :: Security',
		'Topic :: System :: Logging',
		'Topic :: System :: Monitoring',
		'Topic :: System :: Networking',
		'Topic :: System :: Networking :: Monitoring',
		'Topic :: System :: Operating System',
		'Topic :: System :: Operating System Kernels',
	],
)
