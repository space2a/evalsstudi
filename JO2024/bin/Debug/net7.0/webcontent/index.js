let connected = false;

function OnLoad()
{
    document.getElementById("cartbutton").style.display = "none";
    ReloadCart();
    GetAccount();
}


function ReloadCart()
{
    let count = Get("GetCartCount");
    if(count != 0){
        document.getElementById("cartcount").innerText = count + " billet";
        document.getElementById("cartbutton").style.display = "block";

        if(count > 1)
            document.getElementById("cartcount").innerText += "s";
    }
    console.log("BILLETS : " + count);
}

function GoToLoginCreate()
{
    if(!connected)
        document.location.href = document.location.origin + '/logincreate'
    else
        document.location.href = document.location.origin + '/monespace'
}

function GetAccount()
{
    var name = Get("GetAccountName");
    console.log(name);
    if(name == ""){ connected = false; }
    else
    {
        connected = true;
        document.getElementById("createButton").style.display = "none";
        document.getElementById("connectButton").innerText = "Bienvenue " + name;
    }
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
    xmlHttp.open("GET", document.location.origin + "/" + method, false); // false for synchronous request
    xmlHttp.send(null);
    return xmlHttp.responseText;
}

function AddToCart(id)
{
    SendPost("AddToCart", id);
    ReloadCart();
}