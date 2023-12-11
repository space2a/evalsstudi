let connected = false;

function OnLoad(){
    let content = Get("GetCart");
    var items = content.split(/\r?\n|\r|\n/g);

    let empty = true;
    items.forEach(item => {
        if(item == "") return;
        let clone = document.getElementById("base").cloneNode(true);
        clone.id = "";
        clone.getElementsByClassName("cartItemTitle")[0].innerText = item.substring(0, item.indexOf(":"));

        var numbox = clone.getElementsByClassName("numbox")[0];
        numbox.value = item.substring(item.indexOf(":") + 1);
        numbox.addEventListener("change", function(e) 
        {
            SendPost("SetItemCartCount", item.substring(0, item.indexOf(":")) + ":" + numbox.value);
            document.getElementById("total").innerText = "Total : " + Get("GetCartPrice") + "€"; 
        });

        clone.getElementsByClassName("removebutton")[0].addEventListener("click", function(e) 
        {
            SendPost("RemoveFromCart", item.substring(0, item.indexOf(":"))); 
            document.getElementById("total").innerText = "Total : " + Get("GetCartPrice") + "€";
            clone.remove();
        });
        
        document.getElementById("items").appendChild(clone);
        empty = false;
    });

    document.getElementById("base").remove();
    
    if(!empty){
        document.getElementById("empty").style.display = "none";
        document.getElementById("total").innerText = "Total : " + Get("GetCartPrice") + "€";
    }
    else
        document.getElementById("paymentButton").style.display = "none";

    GetAccount();
}

function GetAccount()
{
    var name = Get("GetAccountName");
    console.log(name);
    if(name == ""){ connected = false; document.getElementById("paymentButton").innerText = "Se connecter";}
    else{ connected = true; }
}

function PaymentButton()
{
    if(!connected)
        document.location.href = document.location.origin + '/logincreate';
    else
        document.location.href = document.location.origin + '/Payment'; //Ici vrai code de paiement...
}

function SendPost(method, content)
{
    var xhr = new XMLHttpRequest();
    xhr.open("POST", document.location.origin + "/" + method, false);
    xhr.setRequestHeader('Content-Type', 'application/json');
    xhr.send(content);
}

function Get(method)
{
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", document.location.origin + "/" + method, false); // false for synchronous request
    xmlHttp.send(null);
    return xmlHttp.responseText;
}

function RemoveItemFromCart(title){

}

function OnChange(title){

}