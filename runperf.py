#!/usr/bin/env python3

import os
import platform
import re
import shutil
import subprocess  # Python 3.7 or later
import threading
import time

from pathlib import Path
import xml.etree.ElementTree as ET

# Global Configuration
VERSION = "1.0-alpha.20211107"
RUNPERF = True      # Set to false for testing the statistics

class ProcessPipe(threading.Thread):
    """Print output from processes"""
    # https://gist.github.com/alfredodeza/dcea71d5c0234c54d9b1

    def __init__(self, prefix=""):
        threading.Thread.__init__(self)
        self.daemon = False
        self.process = None
        self.prefix=prefix
        self.fdRead, self.fdWrite = os.pipe()
        self.pipeReader = os.fdopen(self.fdRead)
        self.start()

    def fileno(self):
        return self.fdWrite

    def run(self):
        try:
            for line in iter(self.pipeReader.readline, ''):
                print("{}{}".format(self.prefix, line.strip('\n')), flush=True)
            pass
        except:
            pass

        try:
            self.pipeReader.close()
        except:
            pass

    def close(self):
        try:
            os.close(self.fdWrite)
        except:
            pass

    def stop(self):
        self._stop = True
        self.close()

    def __del__(self):
        try:
            self.stop()
        except:
            pass

        try:
            del self.fdRead
            del self.fdWrite
        except:
            pass

class ProcessExe:
    """Execute a command"""

    def __init__(self, cmd, cwd=None, check=True):
        shell = False
        if platform.system() == "Linux":
            shell = True

        pipeout = ProcessPipe("OUT| ")
        pipeerr = ProcessPipe("ERR| ")
        try:
            process = subprocess.Popen(
                cmd, cwd=cwd,
                stdout=pipeout, stderr=pipeerr,
                universal_newlines=True, shell=shell
            )
        except:
            self.returncode = -1
            pipeout.close()
            pipeerr.close()
            raise

        self.returncode = process.wait()
        time.sleep(0.5)
        pipeout.close()
        pipeerr.close()
        time.sleep(0.5)

        if (check and self.returncode != 0):
            raise subprocess.CalledProcessError(
                returncode=process.returncode, cmd=cmd
            )

    @ staticmethod
    def run(cmd, cwd=None, check=True):
        """Run the command"""
        process = ProcessExe(cmd, cwd, check)
        return process

perfresults = { }
perfsummary = { }

def runperf(folder, executable):
    cwd = Path(os.getcwd())

    if RUNPERF:
        if os.path.exists(folder):
            shutil.rmtree(folder)

        exe = cwd.joinpath(executable)
        if exe.is_file:
            if executable.endswith(".dll"):
                if platform.system() == "Windows":
                    cmd = f"dotnet {exe} -f * --join -e xml"
                else:
                    cmd = f"dotnet {exe} -f '*' --join -e xml"
            if executable.endswith(".exe"):
                if platform.system() == "Windows":
                    cmd = f"{exe} -f * --join -e xml"
                else:
                    os.chmod(exe, 0o777)
                    cmd = f"{exe} -f '*' --join -e xml"

        os.mkdir(folder)
        ProcessExe.run(cmd, cwd=folder)

    results = cwd.joinpath(folder).joinpath("BenchmarkDotNet.Artifacts")
    for root, dirs, files in os.walk(results):
        for name in files:
            if name.endswith(".xml"):
                parseperf(folder, str(Path(root).joinpath(name)), perfresults, perfsummary)

def parseperf(name, perfxml, results, summary):
    root = ET.parse(perfxml).getroot()
    for c in root.findall("./Benchmarks/BenchmarkCase"):
        type = c.find("./Type").text
        method = c.find("./Method").text
        mean = c.find("./Statistics/Mean").text
        median = c.find("./Statistics/Median").text
        stderr = c.find("./Statistics/StandardError").text

        dtype = results.get(type)
        if dtype == None:
            results[type] = { }
            dtype = results[type]
        dmethod = dtype.get(method)
        if dmethod == None:
            dtype[method] = { }
            dmethod = dtype[method]
        dname = dmethod.get(name)
        if dname == None:
            dmethod[name] = { }
            dname = dmethod[name]
        dname["mean"] = "{:.2f}".format(float(mean))
        dname["median"] = "{:.2f}".format(float(median))
        dname["stderr"] = "{:.2f}".format(float(stderr))

    summaryxml = root.find("./HostEnvironmentInfo")
    if summary.get(name) == None:
        summary[name] = { }
        props = summary[name]
        props["BenchmarkDotNetCaption"] = summaryxml.find("./BenchmarkDotNetCaption").text
        props["BenchmarkDotNetVersion"] = summaryxml.find("./BenchmarkDotNetVersion").text
        props["OsVersion"] = summaryxml.find("./OsVersion").text
        props["ProcessorName"] = summaryxml.find("./ProcessorName").text
        props["PhysicalProcessorCount"] = summaryxml.find("./PhysicalProcessorCount").text
        props["LogicalCoreCount"] = summaryxml.find("./LogicalCoreCount").text
        props["PhysicalCoreCount"] = summaryxml.find("./PhysicalCoreCount").text
        props["RuntimeVersion"] = summaryxml.find("./RuntimeVersion").text
        props["Architecture"] = summaryxml.find("./Architecture").text
        props["HasRyuJit"] = summaryxml.find("./HasRyuJit").text

def printperf():
    # Choose from 'mean', 'media', 'stderr'
    perffields = [ "mean", "stderr" ]

    # Get the runs captures to create the header row
    perfrun = { }
    for type in perfresults:
        for method in perfresults[type]:
            for name in perfresults[type][method]:
                if perfrun.get(name) == None:
                    perfrun[name] = True

    perftablehdr = [ "Type", "Method" ]
    perftablehdrlen = len(perftablehdr)
    perfruncol = { }
    for field in perfrun.keys():
        fieldnamematch = re.match(r"Benchmark\.(\S+)", field)
        if (fieldnamematch == None):
            fieldname = field
        else:
            fieldname = fieldnamematch.group(1)

        first = True
        col = len(perftablehdr)
        for stat in perffields:
            if first:
                perfruncol[field] = col
                perftablehdr.append("{} ({})".format(stat, fieldname))
                first = False
            else:
                perftablehdr.append(stat)
            col += 1

    # Fill in the table
    perftable = [ ]
    for type in perfresults:
        for method in perfresults[type]:
            row = [ None ] * len(perftablehdr)
            row[0] = type
            row[1] = method
            for name in perfresults[type][method]:
                col = perfruncol[name]
                entry = 0
                for field in perffields:
                    row[col + entry] = perfresults[type][method][name][field]
                    entry += 1
            perftable.append(row)

    # Print the framework configuration
    for field in perfrun.keys():
        print("```text")
        print(f"Results = {field}")
        print("")

        print("{}=v{} OS={}".format(
            perfsummary[field]["BenchmarkDotNetCaption"],
            perfsummary[field]["BenchmarkDotNetVersion"],
            perfsummary[field]["OsVersion"]
        ))
        print("{}, {} CPU(s), {} logical and {} physical core(s)".format(
            perfsummary[field]["ProcessorName"],
            perfsummary[field]["PhysicalProcessorCount"],
            perfsummary[field]["LogicalCoreCount"],
            perfsummary[field]["PhysicalCoreCount"]
        ))

        if perfsummary[field]["HasRyuJit"] == "True":
            ryu = " RyuJIT"
        else:
            ryu = ""
        print("  [HOST] : {}, {}{}".format(
            perfsummary[field]["RuntimeVersion"],
            perfsummary[field]["Architecture"],
            ryu
        ))
        print("```")
        print("")

    # Pretty print the table as a Markdown file
    perfwidth = [None] * len(perftable[0])
    for col in range(len(perftablehdr)):
        perfwidth[col] = len(perftablehdr[col])

    for row in perftable:
        for col in range(len(row)):
            w = len(row[col])
            if (perfwidth[col] < w):
                perfwidth[col] = w

    # Print the Markdown table
    hdr = "|"
    for col in range(len(perfwidth)):
        hdr += " {} |".format(perftablehdr[col].ljust(perfwidth[col]))
    print(hdr)

    hdr = "|"
    for col in range(perftablehdrlen):
        hdr += ":{}-|".format("".ljust(perfwidth[col], "-"))
    for col in range(perftablehdrlen, len(perftablehdr)):
        hdr += "-{}:|".format("".ljust(perfwidth[col], "-"))
    print(hdr)

    for row in perftable:
        line = "|"
        for col in range(len(row)):
            line += " {} |".format(row[col].ljust(perfwidth[col]))
        print(line)

def main():
    if RUNPERF:
        ProcessExe.run("dotnet build -c Release")
        ProcessExe.run("dotnet test -c Release --no-build")

    runperf("Benchmark.Net48", "test/PathBenchmark/bin/Release/net48/RJCP.IO.PathBenchmark.exe")
    runperf("Benchmark.NetCore31", "test/PathBenchmark/bin/Release/netcoreapp3.1/RJCP.IO.PathBenchmark.dll")
    printperf()

if __name__ == "__main__":
    main()
