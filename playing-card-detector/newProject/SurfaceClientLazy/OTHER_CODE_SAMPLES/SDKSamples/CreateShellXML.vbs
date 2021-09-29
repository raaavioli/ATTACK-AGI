' CreateShellXML.vbs
'
' This script is designed to register a Surface application with Surface Shell so the application can be launched
' from AppLauncher. There are two sets of data needed to perform this registration. The first set is design time 
' data, such as the name of the application's executable, preview image, and icon. The other set is install time
' information, such as where the files are located. 
'
' To perform the registration, the script needs a partially generated XML file. This file should be created at 
' design time and should include information about the name of the executable and preview files. This information
' should be placed in a well formed XML file with tokens representing the unknown information that cannot be 
' known until install time.
'
' When run, the script should receive 4 arguments:
'  1. The path to the partial xml file,
'  2. The directory where the files exist,
'  3. The executable name
'  4. The relative path to the executable directory from the project
'
' The script replaces all instances of the token with the path to the application files. It then saves the updated 
' xml. It then saves the updated xml in shell's program data directory with the file name <executable name>.xml.
' The executable name is the third argument to the script.
' And, if the culture argument is passed, it generates .lnk file to point to the generated xml file.


' Declare Constants
Const OPEN_FILE_FOR_READING = 1
Const OPEN_FILE_FOR_WRITING = 2
Const TOKEN="@ReplaceMe@"

' Get the arguments that were passed to the script
Set Args = WScript.Arguments
XmlFileLocation = Args(0)
CurrentDirectory = Args(1)
ExecutableName = Args(2)
ExeDirPath = Args(3)
Culture = null

if (WScript.Arguments.Count > 4) then
  Culture = Args(4)
end if

' Read in the xml file for the application
Set FileSystem = CreateObject("Scripting.fileSystemObject")
Set InputXml = FileSystem.OpenTextFile(XmlFileLocation, OPEN_FILE_FOR_READING)
Xml = InputXml.ReadAll

' Find the first replacable token in the XML
Position = InStr(1, Xml, TOKEN, 1)

' Replace the token and find the next one. Continue until all tokens have been replaced.
While Position > 0
	Xml = Mid(Xml, 1, Position-1) & CurrentDirectory & Mid (Xml, Position + Len(TOKEN) )
	Position = InStr(Position, Xml, TOKEN, 1)
Wend

XmlDirPath = CurrentDirectory & "\" & ExecutableName & "\" & ExeDirPath

if Not (IsNull(Culture)) then
  XmlDirPath = XmlDirPath & "\" & Culture 
end if

if Not FileSystem.FolderExists(XmlDirPath) then
  FileSystem.CreateFolder(XmlDirPath)
end if

XmlFilePath = XmlDirPath & "\" & ExecutableName & ".xml"

Set OutputFile = FileSystem.CreateTextFile(XmlFilePath , True)
OutputFile.Write(Xml)
OutputFile.Close

if (IsNull(Culture)) then
  ' Get the WshShell object.
  Set Shell = CreateObject("WScript.Shell")

  ' Get collection by using the Environment property.
  Set Environment = Shell.Environment("Process")

  dim ProgramData
  ProgramData = Environment("ProgramData")

  ' Write the file out to disk
  Folder = ProgramData & "\Microsoft\Surface\v2.0\Programs\"

  if Not FileSystem.FolderExists(Folder) then
    FileSystem.CreateFolder(Folder)
  end if

  LinkPath = Folder & "\" & ExecutableName & ".lnk"

  set shellLink = Shell.CreateShortcut(LinkPath)
  shellLink.TargetPath = XmlFilePath
  shellLink.Description = ExecutableName 
  shellLink.WorkingDirectory = XmlDirPath
  shellLink.Save
end if
