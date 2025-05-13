<?php
$conn = new mysqli("localhost", "root", "1234", "certificate_monitor");
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

$sql = "SELECT id, timestamp, process_name,url, window_title, certificate_subject, thumbprint 
        FROM certificate_logs 
        ORDER BY timestamp DESC";
$result = $conn->query($sql);

if ($result->num_rows > 0) {
    while ($row = $result->fetch_assoc()) {
        echo "<tr>";
        echo "<td>" . htmlspecialchars($row["id"]) . "</td>";
        echo "<td>" . htmlspecialchars($row["process_name"] ?? "N/A") . "</td>";   
        echo "<td>" . htmlspecialchars($row["window_title"] ?? "N/A") . "</td>";
		echo "<td>" . htmlspecialchars($row["url"] ?? "N/A") . "</td>";
		echo "<td>" . htmlspecialchars($row["certificate_subject"]) . "</td>";
		echo "<td>" . htmlspecialchars($row["thumbprint"]) . "</td>";
		echo "<td>" . htmlspecialchars($row["timestamp"]) . "</td>";
        
        echo "</tr>";
    }
} else {
    echo "<tr><td colspan='7'>No logs found</td></tr>";
}

$conn->close();
?>
