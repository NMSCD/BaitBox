import logging
import json
import ctypes
from dataclasses import dataclass

from pymhf.core.hooking import disable, on_key_pressed
from pymhf.core.memutils import map_struct
from pymhf.core.mod_loader import ModState
from nmspy import NMSMod
from pymhf.core.calling import call_function
import pymhf.core.hooking
from pymhf import FUNCDEF
from pymhf.core.module_data import module_data
from nmspy.data.common import TkID 
from nmspy.data.common import cTkFixedString
import os

# A utility hook mod used to generate and extract fishing bait values from the game automatically.
# Use: Make sure your input file has all the product IDs properly formatted, Load in game, Open the bait box ONCE and close it to trigger the pointer detection hook, then press "-"

@dataclass
class fishingModState(ModState): #A special class inheriting from ModState which persists between mod Hot Reloads, allowing mod developers to cache pointers, values etc.
    fishingDataAddress = ctypes.c_ulonglong
    baitNameAddress = ctypes.c_ulonglong
    returnEntryAddress = ctypes.c_ulonglong
    fishProdAddress = ctypes.c_ulonglong
    realityManagerAddress = ctypes.c_ulonglong

@disable
class fishyBusiness(NMSMod):
    #General "Nice To Have"s
    __author__ = "ThatBomberBoi"
    __description__ = "Fishy Business"
    __version__ = "0.1"
    __NMSPY_required_version__ = "0.7.0"    
    
    #Create an instance of the persistant ModState Class in your mod class.
    state = fishingModState()    

    def __init__(self):
        super().__init__()
        self.should_print = False   

    #Manual hook to the description generating function. This is required to get the memory pointer for a cGcFishingData instance to pass into the function arguments when we call this function ourselves 
    baitDescFuncDef = FUNCDEF(restype=ctypes.c_ulonglong, argtypes=[ctypes.c_ulonglong, ctypes.c_ulonglong, ctypes.c_ulonglong])
    @pymhf.core.hooking.manual_hook("cGcFishingData::GetBaitBoxDescriptionForBaitID", pattern="48 8B C4 55 41 56 48 8D A8 C8", func_def=baitDescFuncDef, detour_time="after")
    def onBaitBoxDescForBaitIDAcquired(self, this, a2, a3, _result_):
        self.state.fishingDataAddress = this
        self.state.baitNameAddress = a3
        self.state.returnEntryAddress = a2
        logging.info("Bait Hook Run!")

    #WIP: Manual hook to a Fish Description generation function. Need to figure out how to modify the product struct to pass in different IDs w/o instantiating a new struct. Only the ID and Desc seem to be checked by the function  
    fishDescFuncDef = FUNCDEF(restype=ctypes.c_char_p, argtypes=[ctypes.c_ulonglong, ctypes.c_ulonglong, ctypes.c_ulonglong])
    @pymhf.core.hooking.manual_hook("cGcRealityManager::GenerateFishDescription", pattern="48 8B C4 48 89 48 08 55 41 56 41 57 48 8D A8 C8", func_def=fishDescFuncDef, detour_time="after")
    def onFishDescGenerated(self, *args, _result_):
        fixedString = map_struct(args[1], cTkFixedString[0x1024])
        self.state.fishProdAddress = args[2]
        self.state.realityManagerAddress = args[0]
        logging.info(f"Fish Description Generated W/ Input {str(fixedString)}")     

    #WIP: Trying to call the fish description function.
    @on_key_pressed("`")
    def requestCustomFishData(self):
        logging.info("Starting")
        newID = map_struct(self.state.fishProdAddress + 0x128, TkID[0x10])
        newDesc = map_struct(self.state.fishProdAddress + 0x0F8, TkID[0x800])
        logging.info(f"Mapped Fields: {newID.value, newDesc.value}")
        newID.value = self.state.testId
        newDesc.value = self.state.testDesc
        logging.info("Set Fields")
        newOutput = ctypes.create_string_buffer(b"", 1024)
        logging.info("Created String Buffer")
        ret = call_function("cGcRealityManager::GenerateFishDescription", self.state.realityManagerAddress, pymhf.core.memutils.get_addressof(newOutput), self.state.fishProdAddress, pattern="48 8B C4 48 89 48 08 55 41 56 41 57 48 8D A8 C8", func_def=self.fishDescFuncDef)
        logging.info(f"Tried Calling The Gen Function with product ID {newID.value}: {str(newOutput.value)}")


    #Press the "-" key to read the input ID JSON file, and output the generated JSON.
    @on_key_pressed("-")
    def requestCustomBaitData(self):
             descriptions = list()
             cwd = os.path.dirname(__file__)
             with open(os.path.join(cwd, "Data/Input.json"), 'r') as file:
                data = json.load(file)
             product_ids = [item['productId'] for item in data]
             for prod in product_ids:
                newBait = ctypes.create_string_buffer(prod.encode(), 0x128)
                returnEntry = ctypes.create_string_buffer("".encode(), 0x256)
                ret = call_function("cGcFishingData::GetBaitBoxDescriptionForBaitID", self.state.fishingDataAddress, pymhf.core.memutils.get_addressof(returnEntry), pymhf.core.memutils.get_addressof(newBait), pattern="48 8B C4 55 41 56 48 8D A8 C8", func_def=self.baitDescFuncDef)
                logging.info(f"Bait Data For {str(newBait.value)} : {str(map_struct(ret, TkID[0x256]))}")
                descriptions.append(str(map_struct(ret, TkID[0x256]))) #Function returns the description as a tkID<0x256> memory pointer. NMS.py can map this pointer to the struct, which allows one to use utility functions such as str() to convert it to a more standard type.
             with open(os.path.join(cwd, "Data/Output.json"), 'w', encoding='utf-8') as output:
                json.dump(descriptions, output, indent=4)   
                logging.info("Output Ready!")
                 