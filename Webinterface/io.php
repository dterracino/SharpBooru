<?php

require_once("helper.php");

class Reader
{
	private $handle;

	function Reader($data)
	{
		$this->handle = fopen("php://memory", "w+");
		fwrite($this->handle, $data);
		rewind($this->handle);
	}

	function ReadBool() { return $this->ReadByte() <> 0; }

	function ReadByte() { return unpack("C", fread($this->handle, 1))[1]; }

	function ReadBytes() { return fread($this->handle, $this->ReadUInt()); }

	function ReadShort() { return unpack("s", fread($this->handle, 2))[1]; }

	function ReadInt() { return unpack("l", fread($this->handle, 4))[1]; }

	function ReadUShort() { return unpack("n", fread($this->handle, 2))[1]; }

	function ReadUInt() { return unpack("N", fread($this->handle, 4))[1]; }

	function ReadULong()
	{
		// http://stackoverflow.com/a/14406017/1502200
		list($higher, $lower) = array_values(unpack('N2', fread($this->handle, 8))); 
		return $higher << 32 | $lower; 
	}

	function ReadString()
	{
		$len = $this->ReadUShort();
		if ($len > 0)
		{
			$str = fread($this->handle, 2 * $len);
			return enc_ctp($str);
		}
		else return "";
	}

	function close() { fclose($this->handle); }
}

class Writer
{
	private $handle;

	function Writer() { $this->handle = fopen("php://memory", "w+"); }

	function WriteBool($bool)
	{
		if ($bool <> 0)
			$this->WriteByte(1);
		else $this->WriteByte(0);
	}

	function WriteByte($byte)
	{
		$packed = pack("C", $byte);
		fwrite($this->handle, $packed);
	}

	function WriteString($str)
	{
		$length = pack("n", strlen($str));
		$encoded = enc_ptc($str);
		fwrite($this->handle, $length);
		fwrite($this->handle, $encoded);
	}

	//TODO Support for 64Bit
	function WriteULong($ul)
	{
		$this->WriteUInt(0);
		$this->WriteUint($ul);
	}

	function WriteUInt($uint)
	{
		$data = pack("N", $uint);
		fwrite($this->handle, $data);
	}

	function WriteBytes($data)
	{
		$this->WriteUInt(strlen($data));
		fwrite($this->handle, $data);
	}

	function GetBytes()
	{
		fflush($this->handle);
		rewind($this->handle);
		return stream_get_contents($this->handle);
	}

	function close() { fclose($this->handle); }
}

?>
