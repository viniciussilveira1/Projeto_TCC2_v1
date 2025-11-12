using System;
using System.Collections.Generic;

[Serializable]
public class ResponseData
{
    public string questionId;
    public string choiceType; // "correct" | "neutral" | "wrong"
}

[Serializable]
public class AssessmentData
{
    public string studentName;
    public string schoolName;
    public string gradeYear;
    public int finalScore;
    public string sentAt;
    public List<ResponseData> responses;
    
}
