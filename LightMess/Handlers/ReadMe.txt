The special handlers implemented here are correctly implemented to use IO completion ports for
async IO for file, web and SQL Server IO rather than just simple synchronous IO being run on a 
threadpool thread.