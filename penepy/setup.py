from setuptools import setup

setup(name="penepy",
      version="0.0.5",
      packages=["penepy"],
      package_data={'penepy': ['awlib.dll']},
      package_dir={"penepy": "penepy"},
      install_requires=["numpy", "matplotlib", "pythonnet", "pandas"])
