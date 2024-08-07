import os
import re
from git import Repo

# Specify the path to the Git repository
repo_path = "D:\Prasana\Code\Az\eshop"  # Update this path as necessary

# Initialize the repository
repo = Repo(repo_path)

# Define regex patterns to search for logging statements and observability calls
log_patterns = {
    '.py': re.compile(r'\blog\b|\bprint\b|\bdebug\b|\binfo\b|\bwarn\b|\berror\b|\bexception\b|\bsplunk\b|\bdynatrace\b'),
    '.java': re.compile(r'\bSystem\.out\.print\b|\bLogger\b|\bLog\b|\bsplunk\b|\bdynatrace\b'),
    '.cs': re.compile(r'\bConsole\.WriteLine\b|\bDebug\.WriteLine\b|\bTrace\.WriteLine\b|\bLogger\b|\bLog\b|\bsplunk\b|\bdynatrace\b'),
    '.js': re.compile(r'\bconsole\.log\b|\bconsole\.error\b|\bconsole\.warn\b|\bconsole\.info\b|\bsplunk\b|\bdynatrace\b'),
    '.ts': re.compile(r'\bconsole\.log\b|\bconsole\.error\b|\bconsole\.warn\b|\bconsole\.info\b|\bsplunk\b|\bdynatrace\b'),
    '.jsx': re.compile(r'\bconsole\.log\b|\bconsole\.error\b|\bconsole\.warn\b|\bconsole\.info\b|\bsplunk\b|\bdynatrace\b'),
    '.tsx': re.compile(r'\bconsole\.log\b|\bconsole\.error\b|\bconsole\.warn\b|\bconsole\.info\b|\bsplunk\b|\bdynatrace\b')
}

# Function to scan a file for logging statements and observability calls
def scan_file(file_path, log_pattern):
    with open(file_path, 'r', encoding='utf-8') as file:
        lines = file.readlines()
        for i, line in enumerate(lines):
            if log_pattern.search(line):
                print(f"Log/Observability statement found in {file_path} at line {i + 1}: {line.strip()}")

# Walk through the repository files
for root, _, files in os.walk(repo_path):
    for file in files:
        file_extension = os.path.splitext(file)[1]
        if file_extension in log_patterns:
            file_path = os.path.join(root, file)
            scan_file(file_path, log_patterns[file_extension])