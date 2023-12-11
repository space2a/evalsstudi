function CheckEmail(){
    let email = document.getElementById("emailfirst").value;
    console.log("=> " + email);
    if(email == "") return;

    let answer = SendPost("EmailUsed", email);
    document.getElementsByClassName("email")[0].value = email;
    document.getElementsByClassName("email")[1].value = email;

    if(answer == "True")
    {
        //Le compte existe deja
        document.getElementById("title").innerText = "Bon retour !";
        document.getElementById("subtitle").innerText = "Saisissez votre mot de passe !";

        
        document.getElementById("loginAccount").style.display = "block";
    }
    else
    {
        //Existe pas
        document.getElementById("title").innerText = "Bienvenue !";
        document.getElementById("subtitle").innerText = "Complétez votre compte Paris 2024";

        document.getElementById("createAccount").style.display = "block";
    }

    document.getElementById("emailask").style.display = "none";
    document.getElementById("retour").style.display = "block";
    return true;
}


function SendPost(method, content)
{
    var xhr = new XMLHttpRequest();
    xhr.open("POST", document.location.origin + "/" + method, false);
    xhr.setRequestHeader('Content-Type', 'application/json');
    xhr.send(content);
    return xhr.responseText;
}

function Get(method)
{
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", document.location.origin + "/" + method, false);
    xmlHttp.setRequestHeader('Content-Type', 'application/json');
    xmlHttp.send(null);
    return xmlHttp.responseText;
}