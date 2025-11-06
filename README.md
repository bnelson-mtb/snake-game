```
Author: Brady Nelson
Partner: Charlie Adair
Start Date: 8/26/2025
Course: CS 3500, University of Utah, School of Computing
GitHub IDs: bnelson-mtb & big-chuz
Repo: https://github.com/uofu-cs3500-20-fall2025/spreadsheetpair-bradycharlie
Commit Date: 10/29/2025
Solution: Spreadsheet
Copyright: CS 3500, Brady Nelson, Charlie Adair
This work may not be copied for use in Academic Coursework.
```

# Overview of the Spreadsheet functionality
A fully functional spreadsheet engine built in C# that supports core spreadsheet behaviors such as storing, evaluating, 
and saving cells. It can handle text, numbers, and mathematical formulas that reference other cells, automatically 
updating dependent cells when a change occurs. The program tracks cell relationships using a `DependencyGraph`, 
preventing circular dependencies and maintaining recalculation order. The `Formula` class parses and evaluates 
arithmetic expressions with standard infix syntax, including variables and operator precedence. The `Spreadsheet` 
class manages all cells, ensures name validity, and supports saving/loading spreadsheets as JSON files. It also 
implements change tracking, an indexer for cell access (`sheet["A1"]`), and integrates error handing via 
`FormulaError` objects. A web-based GUI application (`GUI` and `GUI.Client`) allows the user to interface 
with the spreadsheet engine as well as save and load the JSON files.

# Time Expenditures:
1. Assignment One - Predicted Hours: 5 - Actual Hours: 7
2. Assignment Two - Predicted Hours: 8 - Actual Hours: 6
3. Assignment Three - Predicted Hours: 7 - Actual Hours: 5.5
4. Assignment Four - Predicted Hours: 6 - Actual Hours: 5.5
5. Assignment Five - Predicted Hours: 6 - Actual Hours: 6
6. Assignment Six - Predicted Hours: 5 - Actual Hours: 9
7. Assignment Seven - Predicted Hours: 8 - Actual Hours: 10

# Examples of Good Software Practice (GSP)
1. **Modular Design**: The program is structured into distinct modules, each handling specific functionalities such as formula parsing, dependency management, and evaluation.
2. **Error Handling**: Comprehensive error handling is implemented to manage invalid inputs. 
3. **Documentation**: The code is well-documented with comments explaining the purpose and functionality of classes and methods.
4. **Version Control**: Updates to the API were first branched and then merged once stable.

# Reflections on the Project
1. **Time Management**: The project required careful time management to balance development and testing phases. Initially, we both tended to either underestimate or overestimate the time needed for assignments. As the project progressed, we realized that tasks like debugging and testing often take significantly more time than expected. We learned to allocate more time specifically for those phases. At the same time, we discovered that implementation is often quick and straightforward when sufficient time is dedicated to design and planning. This approach also made testing and debugging more efficient.
2. **Collaboration**:
	- Working with a partner was beneficial, as having another perspective helped us better understand complex issues. Working together often showed us solutions we might not have considered on our own. We also liked the flexibility of branching our work when we couldn�t meet in person, allowing us to make progress independently and later review and merge our changes. This process gave us a glimpse into how professional software developers collaborate remotely, often without being in the same physical space.
	- One aspect of the teamwork that we could improve on is better coordinating time to work together, such as scheduling several sessions in advance into our calendars. This would allow for more seamless time to work on the project.

# Branch History
#### PS6
**Merge Commit ID**: 472dc21a16979112a6f156197ce20dfc318a3604
This branch included all of assignment 6. Key additions included implementing file saving (`Save`) and loading (`Load`) using JSON serialization, managing a public `Changed` property to track modifications since the last save, and adding indexer support (`ss["A1"]`) for cell value access.
#### fix-save-load-empty-sprd
**Merge Commit ID**:
c137dfb6866780dc78ef8d908ef260e225720045
This branch cleaned up the save process to serialize the entire spreadsheet once, improving efficiency and allowing empty spreadsheets to be saved.

# Use of AI*
**Google NotebookLM**: Used to rephrase and better explain assignment instructions and lectures slides.
**ChatGPT & Google Gemini**: Used to improve syntax and understanding of C#.

```
* See individual project READMEs for more specific descriptions on use of AI.
```