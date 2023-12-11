function Logout()
{
    document.location.href = document.location.origin + '/Logout';
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