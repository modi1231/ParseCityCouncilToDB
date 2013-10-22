using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace ParseCityCouncilToDB
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader sr = null;
            string temp = string.Empty;
            string[] temp_lines;

            //j13-10-08
            //j13-09-17.txt
            string sFileName = "C:\\code\\sandbox\\ParseCityCouncilToDB\\j13-10-08.txt";
            string tempSectionHeader = string.Empty;

            CityCouncilJournal tempJournal = new CityCouncilJournal();

            List<string> condensed = new List<string>();

            Regex oRegex = null;
            MatchCollection oRegexCollection = null;

            try
            {
                // read in the file.
                sr = new StreamReader(sFileName);
                temp = sr.ReadToEnd();
                //remove all the odd carriage returns.
                temp = temp.Replace("\r", string.Empty);
                // save the file name
                tempJournal.FileName = System.IO.Path.GetFileName(sFileName);
                // save the entire file too (backup)
                tempJournal.EntireFile = temp;

                //but all the lines into an array.
                temp_lines = temp.Split('\n');

                // join like lines.
                for (int z = 0; z < temp_lines.Length - 1; z++)
                {
                    if (temp_lines[z].Trim().Length > 0)
                    {

                        for (int y = z + 1; y < temp_lines.Length - 1 && temp_lines[y].Trim().Length > 0; y++)
                        {
                            temp_lines[z] += " " + temp_lines[y];
                            temp_lines[y] = string.Empty;
                        }
                    }
                }

                // remove all the empty lines
                for (int y = 0; y < temp_lines.Length - 1; y++)
                {
                    if (temp_lines[y].Trim().Length > 0)
                    {
                        condensed.Add(temp_lines[y].Trim());
                    }
                }

                //remove page numbers
                oRegex = new System.Text.RegularExpressions.Regex("- [0-9]+ -");
                oRegexCollection = null;

                for (int i = 0; i < condensed.Count - 1; i++)
                {
                    oRegexCollection = oRegex.Matches(condensed[i]);
                    if (oRegexCollection != null && oRegexCollection.Count > 0)
                    {
                        condensed[i] = condensed[i].Replace(oRegexCollection[0].Value.ToString(), string.Empty).Trim();
                    }

                }

                //go through each line and categorize them.
                for (int i = 0; i < condensed.Count - 1; i++)
                {
                    if (i == 0)
                    {
                        tempJournal.Tile = condensed[i];
                        continue;
                    }

                    if (i == 1)
                    {
                        tempJournal.Date = condensed[i];
                        continue;
                    }

                    if (i == 2)
                    {
                        tempJournal.SessionInfo = condensed[i];
                        continue;
                    }

                    if (condensed[i].ToUpper().Trim() == "ROLL CALL")
                    {
                        //skip the roll call line
                        // skip the city clerk calls the roll.
                        i += 2;
                        tempJournal.Roll = condensed[i];
                        continue;
                    }

                    if (condensed[i].Contains("Pledge"))
                    {
                        tempJournal.Pledge = true;
                        continue;
                    }

                    if (condensed[i].Contains("Invocation"))
                    {
                        tempJournal.Invocation = condensed[i];
                        continue;
                    }

                    if (condensed[i].StartsWith("CERTIFICATION OF PUBLICATION"))
                        continue;

                    if (condensed[i].StartsWith("4. "))
                    {
                        //grab #4, the ccid, and the 'a current copy'
                        tempJournal.CertofPubl = condensed[i];
                        i += 1;
                        tempJournal.CertofPubl += "\n" + condensed[i];
                        i += 1;
                        tempJournal.CertofPubl += "\n" + condensed[i];
                        continue;
                    }

                    if (condensed[i].StartsWith("WHENEVER ANY PERSON"))
                    {
                        continue;
                    }

                    if (condensed[i].ToUpper().Trim() == "CONSENT AGENDA")
                    {
                        tempJournal.ConsetAgenda += condensed[i];
                        i += 1;
                        while (condensed[i].StartsWith("("))
                        {
                            tempJournal.ConsetAgenda += "\n" + condensed[i];
                            i += 1;
                        }
                        i -= 1;
                        continue;
                    }

                    // go in to general collection for pursant issues.
                    if (condensed[i].StartsWith("“PURSUANT TO")
                        || condensed[i].StartsWith("("))
                    {
                        tempJournal.PursuantTo.Add(condensed[i]);
                        continue;
                    }

                    if (condensed[i].StartsWith("ADJOURNED"))
                    {
                        tempJournal.Adjourned = condensed[i];
                        i += 1;
                        for (int zz = i; zz < condensed.Count; zz++)
                        {
                            tempJournal.WrapUp += condensed[zz] + "\n";
                            i += 1;
                        }
                        continue;
                    }

                    //get all the sections and their headers..
                    oRegex = new Regex("[0-9]+[.]");
                    oRegexCollection = null;
                    oRegexCollection = oRegex.Matches(condensed[i]);

                    if (oRegexCollection != null && oRegexCollection.Count > 0 && condensed[i].StartsWith(oRegexCollection[0].Value.ToString()))
                    {
                        string uu = string.Empty;
                        uu += condensed[i];
                        i += 1;
                        for (int zz = i; zz < condensed.Count - 1; zz++)
                        {
                            oRegexCollection = null;
                            oRegexCollection = oRegex.Matches(condensed[zz]);

                            if ((oRegexCollection != null && oRegexCollection.Count == 0)
                                && !condensed[zz - 1].StartsWith("“PURSUANT TO")
                                && !condensed[zz].StartsWith("“PURSUANT TO")
                                && !condensed[zz].StartsWith("*  *  *")
                                && !(condensed[zz].ToUpper().Trim() == "CONSENT AGENDA")
                                && !(condensed[zz].ToUpper().Trim() == "EXECUTIVE SESSION")
                                && !condensed[zz].StartsWith("ADJOURNED"))
                            {
                                uu += condensed[zz] + "\n\n";
                                i += 1;
                            }
                            else
                            {
                                if (!condensed[zz].StartsWith("*  *  *"))
                                    i -= 1;
                                break;
                            }
                        }

                        tempJournal.genSections.Add(new GeneralSection(tempSectionHeader, uu));
                        continue;
                    }
                    else
                    {
                        if (!condensed[i].StartsWith("“PURSUANT TO"))
                        {
                            // prob section header
                            tempSectionHeader = condensed[i];
                        }
                    }
                }

                // -- save the data
                DoSave(tempJournal);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        static void DoSave(CityCouncilJournal journal)
        {

            System.Data.SqlClient.SqlCommand cmd = null;
            System.Data.SqlClient.SqlConnection con = null;
            string sql = string.Empty;

            Int32 ID = 0;

            try
            {
                con = new SqlConnection("Server=JEREMY-PC;Database=test;Trusted_Connection=True;");
                con.Open();


                sql = @"INSERT INTO [dbo].[OmahaCityCouncilJournal]   
                        ([Title]    
                        ,[Date]    
                        ,[SessionInfo]   
                        ,[Pledge]   
                        ,[Invocation]   
                        ,[CertificateOfPublication]   
                        ,[ConsentAgenda]   
                        ,[Adjourned]   
                        ,[WrapUp]   
                        ,[FileName])   
                  VALUES   
                       (@Title    
                       ,@Date  
                       ,@SessionInfo
                       ,@Pledge
                       ,@Invocation
                       ,@CertificateOfPublication
                       ,@ConsentAgenda
                       ,@Adjourned
                       ,@WrapUp
                       ,@FileName
                       )";


                cmd = new SqlCommand(sql, con);

                cmd.Parameters.AddWithValue("@Title", journal.Tile);
                cmd.Parameters.AddWithValue("@Date", journal.Date);
                cmd.Parameters.AddWithValue("@SessionInfo", journal.SessionInfo);
                cmd.Parameters.AddWithValue("@Pledge", journal.Pledge);
                cmd.Parameters.AddWithValue("@Invocation", journal.Invocation);
                cmd.Parameters.AddWithValue("@CertificateOfPublication", journal.CertofPubl);
                cmd.Parameters.AddWithValue("@ConsentAgenda", journal.ConsetAgenda);
                cmd.Parameters.AddWithValue("@Adjourned", journal.Adjourned);
                cmd.Parameters.AddWithValue("@WrapUp", journal.WrapUp);
                cmd.Parameters.AddWithValue("@FileName", journal.FileName);

                cmd.ExecuteNonQuery();

                //--------------------------

                cmd = new SqlCommand("SELECT @@IDENTITY", con);

                var temp = cmd.ExecuteScalar();

                if (temp != null)
                    ID = Convert.ToInt32(temp);

                if (ID > 0)
                {
                    sql = @"INSERT INTO [dbo].[OmahaCityCouncilJournal_FileData]
                           ([JournalID]
                           ,[EntireFile])
                     VALUES
                           (@JournalID
                           ,@EntireFile)";
                    cmd = new SqlCommand(sql, con);

                    cmd.Parameters.AddWithValue("@JournalID", ID);
                    cmd.Parameters.AddWithValue("@EntireFile", journal.EntireFile);
                    cmd.ExecuteNonQuery();


                    //--------------------------
                    sql = @"INSERT INTO [dbo].[OmahaCityCouncilJournal_Sections]
                           ([Journal_ID]
                           ,[SectionName]
                           ,[SectionInfo])
                     VALUES
                           (@Journal_ID
                           ,@SectionName
                           ,@SectionInfo)";
                    cmd = new SqlCommand(sql, con);

                    foreach (GeneralSection tempSections in journal.genSections)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@Journal_ID", ID);
                        cmd.Parameters.AddWithValue("@SectionName", tempSections.SectionName);
                        cmd.Parameters.AddWithValue("@SectionInfo", tempSections.Information);
                        cmd.ExecuteNonQuery();
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
            finally
            {
                if (con != null)
                    con.Dispose();
            }

        }
    }
}

