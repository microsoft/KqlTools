"""A script for viewing and transforming real-time event streams with KQL"""
import os
import sys
import traceback
import json
import clr

# Adding reference C# DLL References
REAL_TIME_KQL_LIBRARY = os.path.join(os.path.dirname(__file__), 'lib', 'RealTimeKqlLibrary.dll')
NEWTON_SOFT = os.path.join(os.path.dirname(__file__), 'lib', 'Newtonsoft.Json.dll')
clr.AddReference(REAL_TIME_KQL_LIBRARY)
clr.AddReference(NEWTON_SOFT)

# Importing classes from clr
from RealTimeKqlLibrary import *
from System.Collections.Generic import Dictionary;
from System.Reactive.Kql.CustomTypes import KqlOutput;
from Newtonsoft.Json import JsonConvert;

# ADX output related imports
import threading
from pandas import DataFrame
from datetime import datetime, timedelta
from azure.kusto.data import (KustoConnectionStringBuilder, KustoClient,)
from azure.kusto.ingest import (QueuedIngestClient,IngestionProperties,)

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

class PythonAdxOutput(IOutput):
    __namespace__ = "KqlPython"

    def __init__(self, cluster, database, table, clientId, clientSecret, authority="microsoft.com", resetTable=False):
        self.running = True
        self.batchSize = 10000
        self.flushDuration = timedelta(milliseconds = 1000)
        self.lastUploadTime = datetime.utcnow()
        self.initTable = False
        self.nextBatch = list()
        self.currentBatch = None
        self.lock = threading.Lock()

        self.resetTable = resetTable
        self.database = database
        self.table = table
        self.kcsbData = KustoConnectionStringBuilder.with_aad_application_key_authentication(f"https://{cluster}:443/", clientId, clientSecret, authority)
        self.kcsbIngest = KustoConnectionStringBuilder.with_aad_application_key_authentication(f"https://ingest-{cluster}:443/", clientId, clientSecret, authority)
        self.dataClient = KustoClient(self.kcsbData)
        self.ingestClient = QueuedIngestClient(self.kcsbIngest)
        self.ingestionProps = IngestionProperties(database=database, table=table,)

    def KqlOutputAction(self,kqlOutput: KqlOutput):
        self.OutputAction(kqlOutput.Output)

    def OutputAction(self,dictOutput: Dictionary):
        try:
            if self.running:
                # Convert C# Dictionary to Python dict
                txt = JsonConvert.SerializeObject(dictOutput)
                newEvent = json.loads(txt)
                
                # Initialize table
                if not self.initTable:
                    self.CreateOrResetTable(newEvent)
                    self.initTable = True

                # Check if it's time to upload a batch
                if (len(self.nextBatch) >= self.batchSize) or (datetime.utcnow() > self.lastUploadTime + self.flushDuration):
                    self.UploadBatch()

                self.nextBatch.append(newEvent)
        except:
            self.running = False
            print(sys.exc_info())
            print(traceback.print_exc())

    def OutputError(self,error):
        self.running = False 
        print(error)
    
    def OutputCompleted(self):
        if self.running:
            self.UploadBatch()
        self.running = False

    def Stop(self):
        print('\nCompleted!')
        print('\nThank you for using Real-time KQL!')

    def UploadBatch(self):
        self.lock.acquire()
        try:
            if self.currentBatch != None:
                raise Exception('Upload must not be called before the batch currently being uploaded is completed')

            self.currentBatch = self.nextBatch
            self.nextBatch = list()

            if len(self.currentBatch) > 0:
                df = DataFrame(self.currentBatch)
                self.ingestClient.ingest_from_dataframe(df, ingestion_properties=self.ingestionProps)
                print(f"{len(self.currentBatch)},", end = " ")

            self.currentBatch = None
            self.lastUploadTime = datetime.utcnow()
        except:
            self.running = False
            print(sys.exc_info())
            print(traceback.print_exc())
        finally:
            self.lock.release()
    
    def CreateOrResetTable(self,data):
        if self.resetTable:
            # Dropping table
            self.dataClient.execute(self.database, f".drop table {self.table} ifexists")

        # Create-merge table
        tableMapping = "("
        for item in data:
            tableMapping += f"{item}: {self.GetColumnType(data[item])}, "
        tableMapping = tableMapping[:-2] + ")"
        createMergeTableCommand = f".create-merge table {self.table} " + tableMapping
        self.dataClient.execute(self.database, createMergeTableCommand)

    def GetColumnType(self,item):
        if isinstance(item, str):
            return "string"
        elif isinstance(item, bool):
            return "bool"
        elif isinstance(item, datetime):
            return "datetime"
        elif isinstance(item, timedelta):
            return "timespan"
        elif isinstance(item, (int, bytes, bytearray)):
            return "int"
        elif isinstance(item, float):
            return "real"
        else:
            return "dynamic"
