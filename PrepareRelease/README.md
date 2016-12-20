# Prepare the QuantSA library to be ready to make an Excel installer.

This performs several custom steps.  (for now not all of them hacve been implemented which is why this is a checklist)

 - [x] Check that none of the Excel example sheets contain errors
 - [ ] Check that all scripts that get published run without error (the correctness of the output should be checked in the unit tests)
 - [x] Generate a list of Excel sheets and the QSA functions they contain
 - [x] Generate markdown files from Excel source comments
 - [ ] Check that all Excel comments include an example sheet and the listed example sheet does contain that function
 

The steps that should be performed by the conventional build process are:
 - [ ] Compile source
 - [ ] Run tests
 - [ ] Call the above custom steps 
 - [ ] Generate Doxygen files
 - [ ] Generate HTML from markdown (Jekyll)
 - [ ] Build the installer 
 - [ ] Publish the Jekyll generated HTML
 - [ ] Publish the Doxygen generated HTML

