using System;
using System.IO;

public static class IORepeater
{
    public static bool FileAvailability(string source)
    {
        var file= new FileInfo(source);
        if(!File.Exists(source))return false;
        try
        {
            using(FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
            {
                stream.Close();
            }
        }
        catch (IOException)
        {

            return false;
        }
        return true;
    }

    public static bool FileAvailabilityNoCatch(string source)
    {
        var file = new FileInfo(source);
        if (!File.Exists(source)) return false;

        using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
        {
            stream.Close();
        }
        return true;
    }

    public static bool WaitForFileAvailability(string source)
    {
        try
        {
            return TimeoutAction<bool>(() => FileAvailabilityNoCatch(source));
        }
        catch (Exception e)
        {
            throw new Exception($"Unexpected error when waiting for file {source}.\n{e.Message}");
        }
        return false;
    }

    public static bool FileCopy(string source,string target,bool overwrite=true)
    {
        try
        {
            return TimeoutAction<object>(() => { File.Copy(source, target, overwrite); return null; });
        }
        catch (Exception e)
        {
            throw new Exception($"Could not copy file {source} to {target}.\n{e.Message}");
        }
    }

    public static bool FileMove(string source,string target)
    {
        try
        {
            return TimeoutAction<object>(() => { File.Move(source, target); return null; });
        }
        catch (Exception e)
        {
            throw new Exception($"Could not move file {source} to {target}.\n{e.Message}");
        }
    }

    public static bool FileDelete(string target)
    {
        try
        {
            return TimeoutAction<object>(() => { File.Delete( target); return null; });
        }
        catch (Exception e)
        {
            throw new Exception($"Could not delete file {target} to {target}.\n{e.Message}");
        }
    }

    private static bool TimeoutAction<T>(Func<object> method,int timeout=10000)
    {
        var starttime= DateTime.UtcNow;
        while((DateTime.UtcNow - starttime).TotalMilliseconds < timeout)
        {
            try{
                method();
                Console.WriteLine($"IORepeater: {method.ToString()} executed in {(DateTime.UtcNow - starttime).TotalMilliseconds} milliseconds.");
                return true;
                
            }
            catch (System.IO.IOException exception)
            {
            }
            finally
            {
                GC.Collect();
            }
        }
        throw new Exception($"Operation Timeout reached {timeout}. ");
    }

}