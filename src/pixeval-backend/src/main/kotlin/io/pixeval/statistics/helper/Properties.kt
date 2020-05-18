package io.pixeval.statistics.helper

import java.io.File

object Properties {
    fun read(pos: String): Map<String, String> {
        val lines = File(pos).readLines()
        return lines.map { it.split('=')[0] to it.split('=')[1] }.toMap()
    }
}