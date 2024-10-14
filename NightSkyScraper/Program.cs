using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NightSkyScraper
{
    class Program
    {
        public static Dictionary<String, ClubDetails> ClubDetailsCollection = new Dictionary<string, ClubDetails>();
        public static IWebDriver driver;
        public class ClubDetails
        {
            public String Name { get; set; }
            public String Address { get; set; }
            public String Email { get; set; }
            public String Phone { get; set; }
            public String Website { get; set; }
            public String ContactName { get; set; }
            public String Link { get; set; }
            public String Location { get; set; }
        }

        static void Main(string[] args)
        {
            driver = new ChromeDriver();
            var js = (IJavaScriptExecutor)driver;
            //var site = "https://nightsky.jpl.nasa.gov/club-map.cfm?overridelocation=1";
            var site = "https://astronomy.com/community/groups";
            driver.Navigate().GoToUrl(site);

            //get table body element
            // var tables = driver.FindElements(By.ClassName("nearby_clubs"));

            //foreach (var table in tables)
            //{
            //    js.ExecuteScript("arguments[0].style.border='3px solid red'", table);
            //}
            //the second is the actual table, the first is the map

            // var clubTable = tables[1];

            //clear popup if its there
            var popupClass = "frel_button-close";
            driver.FindElement(By.ClassName(popupClass)).Click();

            // scroll all the way to the bottom
            var body = driver.FindElement(By.CssSelector("body"));
            for (int i = 0; i < 60; i++)
            {
                body.SendKeys(Keys.PageDown);
                System.Threading.Thread.Sleep(100);
                //Console.WriteLine(i);
            }

            //get each club name
            var results = driver.FindElement(By.ClassName("resultSet"));
            var clubs = results.FindElements(By.ClassName("dataItem"));
            int count = 1;
            foreach (var club in clubs)
            {
                var clubElement = club.FindElements(By.ClassName("content"));
                //js.ExecuteScript("arguments[0].style.border='3px solid red'", club);
                var clubDeets1 = clubElement[0].Text.Replace("\n","").Split('\r');
                String ClubName = clubDeets1[0];
                String ClubLocation = "";
                if (clubDeets1.Length > 1)
                {
                    ClubLocation = clubDeets1[1];
                }
                //Console.WriteLine(count + "-" + ClubName);

                String PageLink = club.FindElement(By.CssSelector("a")).GetAttribute("href");

                ClubDetails deets = new ClubDetails { Name = ClubName, Link = PageLink , Location = ClubLocation};
                if (!ClubDetailsCollection.ContainsKey(ClubName))
                {
                    ClubDetailsCollection.Add(ClubName, deets);
                    count++;
                }
            }

            //click each link and get the stuff from the page.
            //create csv headers
            String csvLine = "";
            foreach (var property in typeof(ClubDetails).GetProperties())
            {
               csvLine = csvLine + property.Name + ",";
            }
            string path = @"C:\Users\treki\AmericanScraped.csv";
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(csvLine);
            }

            foreach (var clubName in ClubDetailsCollection.Keys)
            {
                ScrapeClubPage(clubName);
            }
            Console.WriteLine("All details scraped");
            //Console.ReadLine();
            driver.Quit();
        }

        public static void ScrapeClubPage(String clubName)
        {
           
            ClubDetails club = null;
            try
            {
                club = ClubDetailsCollection[clubName];
            }
            catch (Exception e)
            {
                Console.WriteLine(clubName + "not found");
            }

            if (club != null)
            {
                driver.Navigate().GoToUrl(club.Link);

                try
                {
                    var popupClass = "frel_button-close";
                    driver.FindElement(By.ClassName(popupClass)).Click();

                }catch (Exception e) { }

                var details = driver.FindElements(By.ClassName("info"));


                //loop though li for one with the text "Club Website"
                foreach (var li in details)
                {
                    var header = li.Text.Replace("\r", "").Split('\n')[0];
                    //get url to their page
                    if (header == "Website")
                    {
                        club.Website = li.FindElement(By.CssSelector("a")).GetAttribute("href");
                    }

                    try
                    {
                        if (header == "Location")
                        {
                            //Get Address
                            club.Address = li.Text.Replace("Location\r\n","").Replace("\r\n", "||");
                        }
                    }
                    catch (Exception e) { club.Address = "Not Found"; }

                    try
                    {
                        if (header == "Phone")
                        {
                            //get phone
                            club.Phone = li.Text.Replace("Phone: ", "");
                        }
                    }
                    catch (Exception e) { club.Phone = "Not Found"; }

                    try
                    {
                        //get email
                        if (header == "Email")
                        {
                            club.Email = li.FindElement(By.CssSelector("a")).GetAttribute("href").Replace("mailto:", "");
                        }

                    }
                    catch (Exception e) { club.Email = "Not Found"; }

                    try
                    {
                        //get contact names
                        if (header == "Contact Information")
                        {
                            var contacts = li.Text.Replace("Contact Information\r\n", "").Replace("\r\n", "||");
                            club.ContactName = contacts;
                        }
                        //foreach (var contact in contacts)
                        //{
                        //    club.ContactName = club.ContactName + contact.Text + "||";
                        //}
                    }
                    catch (Exception e)
                    {
                        club.ContactName = "Not found";
                    }
                }
                //make string to write
                String csvLine = "";
                foreach (var property in typeof(ClubDetails).GetProperties())
                {
                    var valueObject = property.GetValue(club);
                    if (valueObject == null)
                    {
                        valueObject = "";
                    }
                    String value = (String)valueObject;
                    value = value.Replace(",", " ");

                    csvLine = csvLine + value + ",";
                }
                Console.WriteLine(csvLine);
                //write details to csv
                string path = @"C:\Users\treki\AmericanScraped.csv";
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(csvLine);
                }
            }
        }
    }
}
