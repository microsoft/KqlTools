"""A script for viewing and transforming real-time event streams with KQL"""
import os
import sys
import traceback
import json
import clr

# Adding DLL references
REAL_TIME_KQL_LIBRARY = os.path.join(os.path.dirname(__file__), 'lib', 'RealTimeKqlLibrary.dll')
NEWTON_SOFT = os.path.join(os.path.dirname(__file__), 'lib', 'Newtonsoft.Json.dll')
clr.AddReference(REAL_TIME_KQL_LIBRARY)
clr.AddReference(NEWTON_SOFT)

# Importing classes from clr
from RealTimeKqlLibrary import *
from System.Collections.Generic import Dictionary;
from System.Reactive.Kql.CustomTypes import KqlOutput;
from Newtonsoft.Json import JsonConvert;

def printEvents(event):
    print(event)

class PythonOutput(IOutput):
    __namespace__ = "KqlPython"

    def __init__(self,action=printEvents):
        self.running = True
        self.action = action

    def KqlOutputAction(self,kqlOutput: KqlOutput):
        self.OutputAction(kqlOutput.Output)

    def OutputAction(self,dictOutput: Dictionary):
        try:
            if self.running:
                txt = JsonConvert.SerializeObject(dictOutput)
                self.action(json.loads(txt))
        except:
            self.running = False
            print(sys.exc_info())
            print(traceback.print_exc())

    def OutputError(self,error):
        self.running = False 
        print(error)
    
    def OutputCompleted(self):
        self.running = False

    def Stop(self):
        print('\nCompleted!')
        print('\nThank you for using Real-time KQL!')