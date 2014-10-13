<?php
session_start();

require_once("booru.php");
require_once("config.php");

if (isset($_POST["__logout"]))
{
	$_SESSION["__lgdin"] = false;
	$booru->logout();
	session_destroy();
}
else if (isset($_POST["__un"]) && isset($_POST["__pw"]))
	if ($booru->login($_POST["__un"], $_POST["__pw"]))
	{
		$_SESSION["__lgdin"] = true;
		$_SESSION["__un"] = $_POST["__un"];
		$_SESSION["__pw"] = $_POST["__pw"];
		//TODO Save additional user info
	}

function session_username()
{
	if (session_loggedin())
		return $_SESSION["__un"];
	return NULL;
}

function session_password()
{
	if (session_loggedin())
		return $_SESSION["__pw"];
	return false;
}

function session_loggedin()
{
	if (isset($_SESSION["__lgdin"]))
		return $_SESSION["__lgdin"] === true;
	return false;
}

function session_printform()
{
	if (session_loggedin())
	{
		echo '<form method="POST">';
		echo '<input type="hidden" name="__logout" value="logout">';
		echo session_username();
		echo ' <input type="submit" value="Logout">';
		echo "</form>";
	}
	else
	{
		echo '<form method="POST">';
		echo '<input style="width: 80px;" type="text" name="__un">';
		echo ' <input style="width: 80px;" type="password" name="__pw">';
		echo ' <input type="submit" value="Login">';
		echo "</form>";
	}
}

?>
