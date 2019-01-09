using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using LitJson;

//A class to hold all the data of the program being searched
class Program
{
    public string Title, Description, Subject, CountryOfOrigin, MediaType, Duration;
    public int Index;
    
    public Program(string t, string d, string s, string c, string m, string dur, int i)
    {
        Title = t;
        Description = d;
        Subject = s;
        CountryOfOrigin = c;
        MediaType = m;
        Duration = dur;
        Index = i;
    }
}

public class Menu : MonoBehaviour 
{

    public Canvas SearchUI, ResultsUI, DetailsUI;
    public string UserInput = "";
    public InputField IF1, IF2;
    public Image result;
    public RectTransform Content;
    public Scrollbar Bar;

    private List<Image> clones;
    private bool onSearch = true;
    private int CurrentCount, MaxCount, Counter;
    private List<Program> ProgramList;

	void Start () 
    {
        SearchUI.enabled = true;
        ResultsUI.enabled = false;
        DetailsUI.enabled = false;
        ProgramList = new List<Program>();
        CurrentCount = MaxCount = Counter = 0;
        clones = new List<Image>();
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            print("quit");
            Application.Quit();
        }
    }

    //Back Button
    public void Back()
    {
        if (ResultsUI.enabled == true)
        {
            onSearch = true;
            IF1.text = IF2.text = "";
            ResultsUI.enabled = false;
            SearchUI.enabled = true;
        }
        else if (DetailsUI.enabled == true)
        {
            DetailsUI.enabled = false;
            ResultsUI.enabled = true;
        }
    }

    //Show expanded info for a result
    public void ToDetails(int ind)
    {
        //Originally I tried using this: int ind = this.GetComponentInChildren<NumHolder>().index;
        //But that always got the index of the first button, so I created SendToDetails() in NumHolder
        Text[] details = DetailsUI.GetComponentsInChildren<Text>();
        details[0].text = "Title: " + ProgramList[ind].Title;
        details[1].text = "Description: " + ProgramList[ind].Description;
        details[2].text = "Subject: " + ProgramList[ind].Subject;
        details[3].text = "Country of Origin: " + ProgramList[ind].CountryOfOrigin;
        details[4].text = "Media Type: " + ProgramList[ind].MediaType;
        details[5].text = "Duration: " + ProgramList[ind].Duration;
        ResultsUI.enabled = false;
        DetailsUI.enabled = true;
    }


    //When the user finishes editing the input field, save it
    public void SaveInput(InputField input)
    {
        IF1.text = IF2.text = UserInput = input.text;
    }

    //Check that the input is not empty before starting the coroutine
    public void StartSearch()
    {
        if (onSearch == true)
        {
            if (UserInput != "")
            {
                onSearch = false;
                StartCoroutine(Search());
                SearchUI.enabled = false;
                ResultsUI.enabled = true;
            }
        }
        else
        {
            if (UserInput != "")
            {
                Bar.value = 1;
                Content.sizeDelta = new Vector2(0, 610.2f);
                CurrentCount = 0;
                Counter = 0;
                MaxCount = 0;
                foreach (Image clone in clones)
                {
                    Destroy(clone.gameObject);
                }
                clones.Clear();

                onSearch = false;
                StartCoroutine(Search());
            }
        }
    }

    //Coroutine that searches what the user put in the input field
    IEnumerator Search()
    {
        //Clear out the old search
        ProgramList = new List<Program>();
        result.GetComponentInChildren<Text>().text = "Searching...";
        
        UnityWebRequest Request = 
            //Sends a request to the YLE API using my ID and Key, and filters results to programs that are currently available, ignoring anything scheduled to be added later
            UnityWebRequest.Get("https://external.api.yle.fi/v1/programs/items.json?app_id=1f40b9cc&app_key=04793fc742f2e1b3a0f83fded008ffe1&limit=100&availability=ondemand&q=" + UserInput);

        yield return Request.Send();

        if (Request.isNetworkError)
        {
            Debug.Log(Request.error);
        }

        else
            ParseJSON(Request.downloadHandler.text);
    }

    private void ParseJSON(string JSONText)
    {
        JsonReader text = new JsonReader(JSONText);
        JsonData dat = JsonMapper.ToObject(text);

        if (dat["data"].Count > 0)
        {
            for (int x = 0; x < dat["data"].Count; x++)
            {
                string title, description, subject, country, media, duration;
                //The try-catch's might seem weird, but they fix some bugs I encountered while testing
                try
                {
                    if (dat["data"][x]["title"].Keys.Contains("fi"))
                    {
                        title = dat["data"][x]["title"]["fi"].ToString();
                    }
                    else if (dat["data"][x]["title"].Keys.Contains("sv"))
                    {
                        title = "sv: " + dat["data"][x]["title"]["sv"].ToString();
                    }
                    else
                    {
                        title = "Title not Found";
                    }
                }

                catch
                {
                    title = "Title not Found";
                }

                try
                {
                    if (dat["data"][x]["description"].Keys.Contains("fi"))
                    {
                        description = dat["data"][x]["description"]["fi"].ToString();
                    }
                    else if (dat["data"][x]["description"].Keys.Contains("sv"))
                    {
                        description = "sv: " + dat["data"][x]["description"]["sv"].ToString();
                    }
                    else
                    {
                        description = "Description not Found";
                    }
                }

                catch
                {
                    description = "Description not Found";
                }

                try
                {
                    if (dat["data"][x]["subject"][0]["title"].Keys.Contains("fi"))
                    {
                        subject = dat["data"][x]["subject"][0]["title"]["fi"].ToString();
                    }
                    else if (dat["data"][x]["subject"][0]["title"].Keys.Contains("sv"))
                    {
                        subject = "sv: " + dat["data"][x]["subject"][0]["title"]["sv"].ToString();
                    }
                    else
                    {
                        subject = "Subject not Found";
                    }
                }

                catch
                {
                    subject = "Subject not Found";
                }

                try
                {
                    //The and statement here fixes a niche error I found while testing
                    if (dat["data"][x]["countryOfOrigin"].ToString() != "" && dat["data"][x]["countryOfOrigin"].ToString() != "JsonData array")
                    {
                        country = dat["data"][x]["countryOfOrigin"].ToString();
                    }
                    else
                    {
                        country = "Country of Origin not Found";
                    }
                }

                catch
                {
                    country = "Country of Origin not Found";
                }

                try
                {
                    if (dat["data"][x]["typeMedia"].ToString() != "" && dat["data"][x]["typeMedia"].ToString() != "JsonData array")
                    {
                        media = dat["data"][x]["typeMedia"].ToString();
                    }
                    else
                    {
                        media = "Media Type not Found";
                    }
                }

                catch
                {
                    media = "Media Type not Found";
                }

                try
                {
                    if (dat["data"][x].Keys.Contains("duration"))
                    {
                        //Removes the PT from the string format of PT*M*S
                        duration = dat["data"][x]["duration"].ToString().Substring(2);
                    }
                    else
                    {
                        duration = "Duration not Found";
                    }
                }

                catch
                {
                    duration = "Duration not Found";
                }

                ProgramList.Add(new Program(title, description, subject, country, media, duration, x));
            }
        }
        MaxCount = ProgramList.Count - 1;
        if (MaxCount <= 0)
        {
            result.GetComponentInChildren<Text>().text = "No Results Found";
        }
        else
        {
            //initialize the original result
            result.GetComponentInChildren<Text>().text = ProgramList[0].Title;
            result.GetComponentInChildren<NumHolder>().index = ProgramList[0].Index;
            int difference = MaxCount - CurrentCount;
            //make sure only 10 at a time are added, make sure less are added when it reaches end
            if (difference > 9)
            {
                ExpandList(10);
            }
            else
            {
                ExpandList(difference);
            }
        }
    }

    public void ExpandList(int NumToAdd)
    {
        for(int x = 1; x < NumToAdd; x++)
        {
            CurrentCount++;
            Image clone = Instantiate(result);
            clone.transform.SetParent(ResultsUI.GetComponentInChildren<ScrollRect>().content.transform);
            clone.transform.position = new Vector3(result.transform.position.x, (result.transform.position.y - (120) * CurrentCount), 0);
            clone.GetComponentInChildren<Text>().text = ProgramList[CurrentCount].Title;
            clone.GetComponentInChildren<NumHolder>().index = ProgramList[CurrentCount].Index;
            clones.Add(clone);
            if (CurrentCount > 4)
            {
                Content.sizeDelta = new Vector2(0, Content.rect.height + 120);
            }
        }
        Counter++;
    }

    //Check if slider is low enough to add more objects, then add more
    public void ExpandSlider()
    {
        if (Bar.value < 0.1f && CurrentCount < MaxCount)
        {
            int difference = MaxCount - CurrentCount;

            if (difference > 8)
            {
                ExpandList(10);
            }
            else
            {
                ExpandList(difference);
            }
            Bar.value = (0f+(1f/(float)(Counter)));
        }
    }
}