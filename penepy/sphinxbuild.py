import subprocess
import os
print(os.getcwd())

cmd_api = "sphinx-apidoc -f -o ./doc_source ./doc".split()
cmd_build = "sphinx-build -b html ./doc_source ./doc".split()
subprocess.run(cmd_api)
subprocess.run(cmd_build)