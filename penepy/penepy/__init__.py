import os
import sys
sys.path.append(os.path.dirname(__file__))
from materialList import materialPropertyList
from calc import Calc, CalcAW, CalcAWHVLV, CalcAWLV, CalcForrLV, CalcMBE
from animate import Animate
from material import Material, Penetrator, Target
from core import dicconverter, calc, calc_Vdependent, netArraytonpArray, get_constant
from util import getMaterials, getTandP