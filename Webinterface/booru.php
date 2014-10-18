<?php
require_once("session.php");

require_once("io.php");
require_once("config.php");

class BooruTag
{
	public $tag;
	public $color;
}

class BooruPost
{
	public $user;
	public $source;
	public $rating;
	public $width;
	public $height;
	public $date;
	public $tags;
	public $private;
}

class Booru
{
	private $socket;

	function __construct()
	{
		global $socket_path;
		$this->socket = fsockopen("unix://" . $socket_path, -1, $errno, $errstr);
		if ($this->socket === false)
			throw new Exception($errno . ": " . $errstr);

		if (session_loggedin())
			$this->login(session_username(), session_password());
	}

	function __destruct()
	{
		if (isset($this->socket))
		{
			fwrite($this->socket, "\xFF\xFF");
			fflush($this->socket);
			fclose($this->socket);
			unset($socket);
		}
	}

	function xfread($count)
	{
		$buffer = "";
		while (true)
		{
			$readcount = $count - strlen($buffer);
			if ($readcount > 0)
			{
				$read = fread($this->socket, $readcount);
				if (strlen($read))
					$buffer = $buffer . $read;
				else return $buffer;
			}
			else return $buffer;
		}
	}

	function request($request_code, $payload)
	{
		$length = strlen($payload);
		$buffer = pack("nN", $request_code, $length) . $payload;
		fwrite($this->socket, $buffer);
		fflush($this->socket);
		$buffer = $this->xfread(1);
		$unpacked = unpack("C", $buffer);
		if ($unpacked[1] == 0)
		{
			$buffer = $this->xfread(2);
			$length = unpack("n", $buffer)[1];
			$buffer = $this->xfread(2 * $length);
			throw new Exception(enc_ctp($buffer));
		}
		else
		{
			$buffer = $this->xfread(4);
			$length = unpack("N", $buffer)[1];
			$buffer = $this->xfread($length);
			return $buffer;
		}
	}

	function search($pattern)
	{
		$writer = new Writer();
		$writer->WriteString($pattern);
		$response = $this->request(20, $writer->GetBytes());
		$writer->close();
		$reader = new Reader($response);
		$count = $reader->ReadUInt();
		$array = array();
		for ($i = 0; $i < $count; $i++)
		{
			$element = $reader->ReadULong();
			array_push($array, $element);
		}
		return $array;
	}

	function searchTags($term)
	{
		$writer = new Writer();
		$writer->WriteString($term);
		$response = $this->request(24, $writer->GetBytes());
		$writer->close();
		$reader = new Reader($response);
		$count = $reader->ReadUInt();
		$tags = array();
		for ($i = 0; $i < $count; $i++)
		{
			$reader->ReadULong(); //ID
			$tag = new BooruTag();
			$tag->tag = $reader->ReadString();
			$reader->ReadString(); //Type
			$reader->ReadString(); //Description
			$color = $reader->ReadUInt();
			$tag->color = "rgb(" . ($color >> 16 & 255) . "," . ($color >> 8 & 255) . "," . ($color & 255) . ")";
			array_push($tags, $tag);
		}
		$reader->close();
		return $tags;
	}

	function getPost($id)
	{
		$post = new BooruPost();
		$writer = new Writer();
		$writer->WriteULong($id);
		$response = $this->request(0, $writer->GetBytes());
		$id_data = $writer->GetBytes();
		$writer->close();		
		$reader = new Reader($response);
		$reader->ReadULong(); //ID
		$post->user = $reader->ReadString();
		$post->private = $reader->ReadBool();
		$post->source = $reader->ReadString();
		$post->description = $reader->ReadString();
		$post->rating = $reader->ReadByte();
		$post->width = $reader->ReadUInt();
		$post->height = $reader->ReadUInt();
		$post->date = $reader->ReadUInt();
		$reader->close();

		$response = $this->request(6, $id_data);
		$reader = new Reader($response);
		$count = $reader->ReadUInt();
		$post->tags = array();
		for ($i = 0; $i < $count; $i++)
		{
			$reader->ReadULong(); //ID
			$tag = new BooruTag();
			$tag->tag = $reader->ReadString();
			$reader->ReadString(); //Type
			$reader->ReadString(); //Description
			$color = $reader->ReadUInt();
			$tag->color = "rgb(" . ($color >> 16 & 255) . "," . ($color >> 8 & 255) . "," . ($color & 255) . ")";
			array_push($post->tags, $tag);
		}
		$reader->close();

		return $post;
	}

	function login($username, $password)
	{
		$writer = new Writer();
		$writer->WriteString($username);
		$writer->WriteString($password);
		$data = $writer->GetBytes();
		$writer->close();

		try
		{
			$this->request(22, $data);
			return true;
		}
		catch (Exception $ex) { return false; }
	}

	function logout() { $this->request(23, ""); }

	function addPost($user, $tags, $source, $description, $rating, $private, $data)
	{
		$writer = new Writer();

		//Write BooruPost
		$writer->WriteULong(0); //ID
		$writer->WriteString($user); //User
		$writer->WriteBool($private);
		$writer->WriteString($source);
		$writer->WriteString($description);
		$writer->WriteByte($rating);
		$writer->WriteULong(0); //Width + Height
		$writer->WriteUInt(time()); //CreationDate
		$writer->WriteULong(0); //ViewCount
		$writer->WriteULong(0); //EditCount
		$writer->WriteULong(0); //Score
		$writer->WriteUInt(0); //ImageHash length

		//Write BooruTagList
		$tags = explode(" ", $tags);
		$writer->WriteUInt(count($tags));
		foreach ($tags as $tag)
		{
			$writer->WriteULong(0);
			$writer->WriteString($tag);
			$writer->WriteString(""); //Type
			$writer->WriteString(""); //Description
			$writer->WriteUInt(0); //Color
		}

		//Write BooruImage
		$writer->WriteBytes($data);
		$data = $writer->GetBytes();
		$writer->close();

		$response = $this->request(30, $data);
		$reader = new Reader($response);
		$ulong = $reader->ReadULong();
		$reader->close();
		return $ulong;
	}
}

$booru = new Booru();

?>
