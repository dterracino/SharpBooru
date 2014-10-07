<?php
header("Content-Type: text/css");

require_once("helper.php");
require_once("config.php");

cache_headers(12 * 3600);

?>

body  {
    background-color: #2c2c2c;
    margin: 0px;
}

a {
    text-decoration: none;
    color: inherit;
}

div.main {
    color: white;
    font-size: 16px;
    font-family: Arial, "DeJaVu Sans";
    max-width: 100%;
}

div.header {
    position: fixed;
    left: 0px;
    top: 0px;
    width: 100%;
    height: 88px;
    white-space: nowrap;
    overflow: hidden;
    box-shadow: 0px 0px 16px black;
    background-color: black;
    background-image: -moz-linear-gradient(
        45deg, 
        rgba(76,76,76,1) 0%,
        rgba(89,89,89,1) 12%,
        rgba(102,102,102,1) 25%,
        rgba(71,71,71,1) 39%,
        rgba(44,44,44,1) 50%,
        rgba(0,0,0,1) 51%,
        rgba(17,17,17,1) 60%,
        rgba(43,43,43,1) 76%,
        rgba(28,28,28,1) 91%,
        rgba(19,19,19,1) 100%);
}

div.innerheader {
    margin-left: 10px;
    margin-top: 5px;
}

div.login {
    position: absolute;
    top: 10px;
    right: 10px;
}

span.hl {
    font-size: 42px;
    font-weight: bolder;
}

div.sub {
    margin-bottom: 22px;
}

div.sub h2 {
    font-size: 22px;
    font-weight: bolder;
    border-bottom: 1px solid white;
    margin: 0px;
    margin-bottom: 5px;
}

div.sub p {
    margin: 0px;
    padding-left: 10px;
    padding-right: 3px;
}

div.body {
    padding: 110px 10px 10px 10px;
    height: 100%;
}

div.motd {
    position: absolute;
    right: 5px;
    bottom: 5px;
    font-size: 12px;
}

div.login {
    position: absolute;
    right: 5px;
    top: 5px;
}

div.wrap {
    overflow: hidden;
    padding: 0px;
    display: table;
}

div.thumb img {
    max-width: <?php echo $thumb_size; ?>px;
    max-height: <?php echo $thumb_size; ?>px;
    vertical-align: middle;
}

div.thumb {
    width: <?php echo $thumb_size; ?>px;
    height: <?php echo $thumb_size; ?>px;
    float: left;
    margin: 6px;
    line-height: <?php echo $thumb_size; ?>px;
    text-align: center;
}

table.vtable {
    max-width: 100%;
}

td {
    text-align: left;
    vertical-align: top;
    word-break: break-all;
    word-wrap: break-word;
}

td.nav {
    min-width: 200px;
    max-width: 200px;
}

img.mimg {
    max-width: 800px;
    max-height: 800px;
    display: block;
}

input[type=button], input[type=submit] {
    height: 27px;
    color: white;
    font-size: 13px;
    border: 3px double #9c9c9c;
    background-color: #4d4d4d;
}

input[type=text], input[type=password], input[type=number] {
    height: 15px;
}

textarea {
    font: inherit;
}

input[type=text], input[type=password], textarea, input[type=number] {
    color: white;
    font-size: 13px;
    border: 3px double #9c9c9c;
    background-color: #4d4d4d;
    padding: 3px;
}

input[type=text].search {
    width: 189px;
}

input[type=text]:focus, input[type=password]:focus, textarea:focus, {
    background-color: #666666;
}

input.login {
    float: left;
    width: 80px;
    margin-right: 5px;
}

div.page_chooser {
    text-align: center;
    margin-left: auto;
    margin-right: auto;
    width: 80%;
}

span.pc_page {
    line-height: 20px;
    height: 20px;
    text-align: center;
    vertical-align: middle;
    display: inline-block;
    margin-left: 2px;
    margin-right: 2px;
    padding-left: 2px;
    padding-right: 2px;
    min-width: 16px;
    border: 1px solid white;
}

span.pc_selected {
    width: 100%;
    height: 100%;
    font-weight: 800;
    color: #9c9c9c;
}

span.pc_arrow {
    font-size: 28px;
}
