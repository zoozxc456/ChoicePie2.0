<template>
  <div class="relative flex-1 min-h-0">
    <Line
      :data="chartData"
      :options="chartOptions"
    />
  </div>
</template>

<script setup lang="ts">
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Tooltip,
  Legend
} from 'chart.js'
import { Line } from 'vue-chartjs'
import type { DailyCountDto } from '~/types/api'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Tooltip, Legend)

interface Props {
  label: string
  color: string
  dataByDay: DailyCountDto[]
}

const props = defineProps<Props>()

const formatDayLabel = (date: string) => {
  const [, month, day] = date.split('-')
  return `${month}/${day}`
}

const chartData = computed(() => ({
  labels: props.dataByDay.map(d => formatDayLabel(d.date)),
  datasets: [
    {
      label: props.label,
      data: props.dataByDay.map(d => d.count),
      borderColor: props.color,
      backgroundColor: props.color,
      tension: 0.3
    }
  ]
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false }
  },
  scales: {
    y: {
      beginAtZero: true,
      ticks: { precision: 0 }
    }
  }
}
</script>

<style scoped lang="scss"></style>
