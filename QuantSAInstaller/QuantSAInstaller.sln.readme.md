An minimal custom installer that does not require install shield.

 * Builds a normal executable.
 * All the files required to be included in the install directory are specified in InstallFileInfo.csv and Resources.resx, these are updated by running the PrepareRelease solution.
 * The official build does not run the PrepareRelease step, this needs to be run by developers before they commit to master.
 * The local build of QuantSA does not produce zipped_dlls.zip if you want to create an installer locally you need to create this zipped folder by hand.
 * The installer when it is compiled and runs adds QuantSA.xll and ExcelDna.IntelliSense.xll into the registry for excel and copies all files into the selected folder.
 
 
 
 