namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FtpNet : FtpBase
{
    /// <summary>
    ///     OK
    ///     LIST
    ///     Toto je vstupní metoda, metodu getFSEntriesListRecursively s 5ti parametry nevolej, ač má stejný název
    ///     Vrátí soubory i složky, ale pozor, složky jsou vždycky až po souborech
    /// </summary>
    /// <param name = "slozkyNeuploadovatAVS"></param>
    public override Dictionary<string, List<string>> getFSEntriesListRecursively(List<string> slozkyNeuploadovatAVS)
    {
        // Musí se do ní ukládat cesta k celé složce, nikoliv jen název aktuální složky
        var projeteSlozky = new List<string>();
        var vr = new Dictionary<string, List<string>>();
        var fse = ListDirectoryDetails();
        var actualPath = ps.ActualPath;
        OnNewStatus("Získávám rekurzivní seznam souborů ze složky" + " " + actualPath);
        foreach (var item in fse)
        {
            var fz = item[0];
            if (fz == '-')
            {
                if (vr.ContainsKey(actualPath))
                {
                    vr[actualPath].Add(item);
                }
                else
                {
                    var ppk = new List<string>();
                    ppk.Add(item);
                    vr.Add(actualPath, ppk);
                }
            }
            else if (fz == 'd')
            {
                var folderName = SHJoin.JoinFromIndex(8, ' ', SHSplit.Split(item, ""));
                if (!FtpHelper.IsThisOrUp(folderName))
                {
                    if (vr.ContainsKey(actualPath))
                    {
                        vr[actualPath].Add(item + "/");
                    }
                    else
                    {
                        var ppk = new List<string>();
                        ppk.Add(item + "/");
                        vr.Add(actualPath, ppk);
                    }
                //getFSEntriesListRecursively(slozkyNeuploadovatAVS, projeteSlozky, vr, ps.ActualPath, folderName);
                }
            }
            else
            {
                throw new Exception("Nepodporovaný typ objektu");
            }
        }

        return vr;
    }

    /// <summary>
    ///     OK
    ///     Tuto metodu nepoužívej, protože fakticky způsobuje neošetřenou výjimku, pokud již cesta bude skutečně / a a nebude
    ///     moci se přesunout nikde výš
    /// </summary>
    public override void goToUpFolderForce()
    {
        if (FtpLogging.GoToUpFolder)
            OnNewStatus("Přecházím do nadsložky" + " " + ps.ActualPath);
        ps.RemoveLastTokenForce();
        OnNewStatusNewFolder();
    }

    /// <summary>
    ///     OK
    /// </summary>
    public override void goToUpFolder()
    {
        if (ps.CanGoToUpFolder)
        {
            ps.RemoveLastToken();
            OnNewStatusNewFolder();
        }
        else
        {
            OnNewStatus("Nemohl jsem přejít do nadsložky" + ".");
        }
    }

    public override void DebugActualFolder()
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void D(string what, string text, params object[] args)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void Connect()
    {
        ThrowEx.NotImplementedMethod();
    }
}