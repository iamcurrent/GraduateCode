/*
* MATLAB Compiler: 7.1 (R2019b)
* Date: Thu Dec 30 09:04:05 2021
* Arguments:
* "-B""macro_default""-W""dotnet:FindPeaks,Class1,4.0,private,version=1.0""-T""link:lib""-
* d""D:\data_software\DataSystem\FindPeaks\for_testing""-v""class{Class1:D:\data_software\
* DataSystem\DataSystem\bin\x64\Debug\FindPeaks.m}"
*/
using System;
using System.Reflection;
using System.IO;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;

#if SHARED
[assembly: System.Reflection.AssemblyKeyFile(@"")]
#endif

namespace FindPeaks
{

  /// <summary>
  /// The Class1 class provides a CLS compliant, MWArray interface to the MATLAB
  /// functions contained in the files:
  /// <newpara></newpara>
  /// D:\data_software\DataSystem\DataSystem\bin\x64\Debug\FindPeaks.m
  /// </summary>
  /// <remarks>
  /// @Version 1.0
  /// </remarks>
  public class Class1 : IDisposable
  {
    #region Constructors

    /// <summary internal= "true">
    /// The static constructor instantiates and initializes the MATLAB Runtime instance.
    /// </summary>
    static Class1()
    {
      if (MWMCR.MCRAppInitialized)
      {
        try
        {
          Assembly assembly= Assembly.GetExecutingAssembly();

          string ctfFilePath= assembly.Location;

		  int lastDelimiter = ctfFilePath.LastIndexOf(@"/");

	      if (lastDelimiter == -1)
		  {
		    lastDelimiter = ctfFilePath.LastIndexOf(@"\");
		  }

          ctfFilePath= ctfFilePath.Remove(lastDelimiter, (ctfFilePath.Length - lastDelimiter));

          string ctfFileName = "FindPeaks.ctf";

          Stream embeddedCtfStream = null;

          String[] resourceStrings = assembly.GetManifestResourceNames();

          foreach (String name in resourceStrings)
          {
            if (name.Contains(ctfFileName))
            {
              embeddedCtfStream = assembly.GetManifestResourceStream(name);
              break;
            }
          }
          mcr= new MWMCR("",
                         ctfFilePath, embeddedCtfStream, true);
        }
        catch(Exception ex)
        {
          ex_ = new Exception("MWArray assembly failed to be initialized", ex);
        }
      }
      else
      {
        ex_ = new ApplicationException("MWArray assembly could not be initialized");
      }
    }


    /// <summary>
    /// Constructs a new instance of the Class1 class.
    /// </summary>
    public Class1()
    {
      if(ex_ != null)
      {
        throw ex_;
      }
    }


    #endregion Constructors

    #region Finalize

    /// <summary internal= "true">
    /// Class destructor called by the CLR garbage collector.
    /// </summary>
    ~Class1()
    {
      Dispose(false);
    }


    /// <summary>
    /// Frees the native resources associated with this object
    /// </summary>
    public void Dispose()
    {
      Dispose(true);

      GC.SuppressFinalize(this);
    }


    /// <summary internal= "true">
    /// Internal dispose function
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
      if (!disposed)
      {
        disposed= true;

        if (disposing)
        {
          // Free managed resources;
        }

        // Free native resources
      }
    }


    #endregion Finalize

    #region Methods

    /// <summary>
    /// Provides a single output, 0-input MWArrayinterface to the FindPeaks MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <returns>An MWArray containing the first output argument.</returns>
    ///
    public MWArray FindPeaks()
    {
      return mcr.EvaluateFunction("FindPeaks", new MWArray[]{});
    }


    /// <summary>
    /// Provides a single output, 1-input MWArrayinterface to the FindPeaks MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="data">Input argument #1</param>
    /// <returns>An MWArray containing the first output argument.</returns>
    ///
    public MWArray FindPeaks(MWArray data)
    {
      return mcr.EvaluateFunction("FindPeaks", data);
    }


    /// <summary>
    /// Provides a single output, 2-input MWArrayinterface to the FindPeaks MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="data">Input argument #1</param>
    /// <param name="mh">Input argument #2</param>
    /// <returns>An MWArray containing the first output argument.</returns>
    ///
    public MWArray FindPeaks(MWArray data, MWArray mh)
    {
      return mcr.EvaluateFunction("FindPeaks", data, mh);
    }


    /// <summary>
    /// Provides a single output, 3-input MWArrayinterface to the FindPeaks MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="data">Input argument #1</param>
    /// <param name="mh">Input argument #2</param>
    /// <param name="mpd">Input argument #3</param>
    /// <returns>An MWArray containing the first output argument.</returns>
    ///
    public MWArray FindPeaks(MWArray data, MWArray mh, MWArray mpd)
    {
      return mcr.EvaluateFunction("FindPeaks", data, mh, mpd);
    }


    /// <summary>
    /// Provides the standard 0-input MWArray interface to the FindPeaks MATLAB function.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public MWArray[] FindPeaks(int numArgsOut)
    {
      return mcr.EvaluateFunction(numArgsOut, "FindPeaks", new MWArray[]{});
    }


    /// <summary>
    /// Provides the standard 1-input MWArray interface to the FindPeaks MATLAB function.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="data">Input argument #1</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public MWArray[] FindPeaks(int numArgsOut, MWArray data)
    {
      return mcr.EvaluateFunction(numArgsOut, "FindPeaks", data);
    }


    /// <summary>
    /// Provides the standard 2-input MWArray interface to the FindPeaks MATLAB function.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="data">Input argument #1</param>
    /// <param name="mh">Input argument #2</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public MWArray[] FindPeaks(int numArgsOut, MWArray data, MWArray mh)
    {
      return mcr.EvaluateFunction(numArgsOut, "FindPeaks", data, mh);
    }


    /// <summary>
    /// Provides the standard 3-input MWArray interface to the FindPeaks MATLAB function.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="data">Input argument #1</param>
    /// <param name="mh">Input argument #2</param>
    /// <param name="mpd">Input argument #3</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public MWArray[] FindPeaks(int numArgsOut, MWArray data, MWArray mh, MWArray mpd)
    {
      return mcr.EvaluateFunction(numArgsOut, "FindPeaks", data, mh, mpd);
    }


    /// <summary>
    /// Provides an interface for the FindPeaks function in which the input and output
    /// arguments are specified as an array of MWArrays.
    /// </summary>
    /// <remarks>
    /// This method will allocate and return by reference the output argument
    /// array.<newpara></newpara>
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return</param>
    /// <param name= "argsOut">Array of MWArray output arguments</param>
    /// <param name= "argsIn">Array of MWArray input arguments</param>
    ///
    public void FindPeaks(int numArgsOut, ref MWArray[] argsOut, MWArray[] argsIn)
    {
      mcr.EvaluateFunction("FindPeaks", numArgsOut, ref argsOut, argsIn);
    }



    /// <summary>
    /// This method will cause a MATLAB figure window to behave as a modal dialog box.
    /// The method will not return until all the figure windows associated with this
    /// component have been closed.
    /// </summary>
    /// <remarks>
    /// An application should only call this method when required to keep the
    /// MATLAB figure window from disappearing.  Other techniques, such as calling
    /// Console.ReadLine() from the application should be considered where
    /// possible.</remarks>
    ///
    public void WaitForFiguresToDie()
    {
      mcr.WaitForFiguresToDie();
    }



    #endregion Methods

    #region Class Members

    private static MWMCR mcr= null;

    private static Exception ex_= null;

    private bool disposed= false;

    #endregion Class Members
  }
}
